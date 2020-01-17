using MySql.Data.MySqlClient;
using SyncDataDeduct.Classes.Constant;
using SyncDataDeduct.Classes.DataModel;
using SyncDataDeduct.Classes.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SyncDataDeduct.Classes.DB
{
    class Database
    {
        private MySqlConnection localConnection;
        private MySqlConnection serverConnection;
        private string LocalDBHost { get; set; }
        private string LocalDBName { get; set; }
        private string LocalDBUsername { get; set; }
        private string LocalDBPassword { get; set; }
        private string ServerDBHost { get; set; }
        private string ServerDBName { get; set; }
        private string ServerDBUsername { get; set; }
        private string ServerDBPassword { get; set; }

        private List<int> TransactionIds = new List<int>();
        private List<int> TransactionDetailIds = new List<int>();

        public Database()
        {
            Initialize();
        }

        private void Initialize()
        {
            LocalDBHost = DataConfig.LocalDBHost;
            LocalDBName = DataConfig.LocalDBName;
            LocalDBUsername = DataConfig.LocalDBUsername;
            LocalDBPassword = DataConfig.LocalDBPassword;
            InitializeLocalConnection();

            ServerDBHost = DataConfig.ServerDBHost;
            ServerDBName = DataConfig.ServerDBName;
            ServerDBUsername = DataConfig.ServerDBUsername;
            ServerDBPassword = DataConfig.ServerDBPassword;
            InitializeServerConnection();
        }

        private void InitializeLocalConnection()
        {
            string connectionString = "SERVER=" + LocalDBHost + ";" + "DATABASE=" + LocalDBName + ";" + "UID=" + LocalDBUsername + ";" + "PASSWORD=" + LocalDBPassword + ";";
            localConnection = new MySqlConnection(connectionString);
        }

        private void InitializeServerConnection()
        {
            string connectionString = "SERVER=" + ServerDBHost + ";" + "DATABASE=" + ServerDBName + ";" + "UID=" + ServerDBUsername + ";" + "PASSWORD=" + ServerDBPassword + ";";
            serverConnection = new MySqlConnection(connectionString);
        }

        private bool OpenConnection(string type)
        {
            try
            {
                if (type == "local")
                {
                    localConnection.Open();
                }
                else
                {
                    serverConnection.Open();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine(ConstantVariable.ERROR_MESSAGE_CANNOT_ESTABLISH_CONNECTION_TO_SERVER);
                        break;

                    case 1045:
                        Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INVALID_USERNAME_PASSWORD);
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection(string type)
        {
            try
            {
                if (type == "local")
                {
                    localConnection.Close();
                }
                else
                {
                    serverConnection.Close();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public long Insert(string type, string query_cmd, Dictionary<string, object> parameterValues)
        {
            string query = query_cmd;
            long insert_last_id = -1;
            if (this.OpenConnection(type))
            {
                MySqlCommand cmd = type == "local" ? new MySqlCommand(query, localConnection) : new MySqlCommand(query, serverConnection);
                if (parameterValues.Count > 0)
                {
                    foreach (var param in parameterValues)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }
                cmd.ExecuteNonQuery();
                insert_last_id = cmd.LastInsertedId;
                this.CloseConnection(type);
            }
            return insert_last_id;
        }

        public void Update(string type, string query_cmd)
        {
            string query = query_cmd;
            if (this.OpenConnection(type) == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = query;
                cmd.Connection = type == "local" ? localConnection : serverConnection;

                cmd.ExecuteNonQuery();

                this.CloseConnection(type);
            }
        }

        public void Delete(string type, string query_cmd)
        {
            string query = query_cmd;
            if (this.OpenConnection(type) == true)
            {
                MySqlCommand cmd = type == "local" ? new MySqlCommand(query, localConnection) : new MySqlCommand(query, serverConnection);
                cmd.ExecuteNonQuery();
                this.CloseConnection(type);
            }
        }

        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                string path;
                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    LocalDBUsername, LocalDBPassword, LocalDBHost, LocalDBName);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException)
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_UNABLE_TO_BACKUP);
            }
        }

        public void Restore()
        {
            try
            {
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    LocalDBUsername, LocalDBPassword, LocalDBHost, LocalDBName);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException)
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_UNABLE_TO_RESTORE);
            }
        }

        public bool CheckMySQLConnection(string type)
        {
            bool successful;
            try
            {
                successful = this.OpenConnection(type);
                if (successful)
                {
                    this.CloseConnection(type);
                }
            }
            catch (MySqlException)
            {
                successful = false;
            }
            return successful;
        }

        private List<Transaction> FetchUnsyncDataTransaction()
        {
            List<Transaction> transactions = new List<Transaction>();
            if (OpenConnection("local"))
            {
                string query = "select * from deduct_card_results where has_synced = 0 limit " + DataConfig.LimitSyncData;

                MySqlCommand command = new MySqlCommand(query, localConnection);
                MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    int id = Convert.ToInt32(dataReader["id"].ToString());
                    TransactionIds.Add(id);

                    int? parkingOutId = !string.IsNullOrEmpty(dataReader["parking_out_id"].ToString()) ? Convert.ToInt32(dataReader["parking_out_id"].ToString()) : (int?)null;
                    int? parkingInId = !string.IsNullOrEmpty(dataReader["parking_in_id"].ToString()) ? Convert.ToInt32(dataReader["parking_in_id"].ToString()) : (int?)null;
                    string deductResult = dataReader["result"].ToString();
                    string transactionDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(dataReader["transaction_dt"].ToString());
                    int amount = Convert.ToInt32(dataReader["amount"].ToString());
                    string ipv4 = dataReader["ipv4"].ToString();
                    string operatorName = dataReader["operator"].ToString();
                    string idReader = dataReader["ID_reader"].ToString();
                    string bank = dataReader["bank"].ToString();
                    int hasSynced = Convert.ToInt32(dataReader["has_synced"].ToString());
                    string created = TKHelper.ConvertDatetimeToDefaultFormatMySQL(dataReader["created"].ToString());
                    transactions.Add(new Transaction(id, parkingOutId, parkingInId, deductResult, amount, transactionDt, bank, ipv4, operatorName, idReader, hasSynced, created));
                }
                CloseConnection("local");
            }
            return transactions;
        }

        private List<TransactionDetail> FetchUnsyncDataTransactionDetail()
        {
            List<TransactionDetail> transactionDetails = new List<TransactionDetail>();
            if (OpenConnection("local"))
            {
                string query = "select * from deduct_card_result_details where has_synced = 0 limit " + DataConfig.LimitSyncData;
                MySqlCommand command = new MySqlCommand(query, localConnection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int deductCardResultDetailId = Convert.ToInt32(reader["id"].ToString());
                    TransactionDetailIds.Add(deductCardResultDetailId);

                    int deductCardResultId = Convert.ToInt32(reader["deduct_card_result_id"].ToString());
                    int? peopleTicketId = !string.IsNullOrEmpty(reader["people_ticket_id"].ToString()) ? Convert.ToInt32(reader["people_ticket_id"].ToString()) : (int?)null;
                    int? cargoFareId = !string.IsNullOrEmpty(reader["cargo_fare_id"].ToString()) ? Convert.ToInt32(reader["cargo_fare_id"].ToString()) : (int?)null;
                    int amount = Convert.ToInt32(reader["amount"].ToString());
                    int hasSynced = Convert.ToInt32(reader["has_synced"].ToString());
                    string created = reader["created"].ToString();
                    transactionDetails.Add(new TransactionDetail(deductCardResultDetailId, deductCardResultId, peopleTicketId, cargoFareId, amount, hasSynced, created));
                }
                CloseConnection("local");
            }
            return transactionDetails;
        }

        public void SyncDataToServer()
        {
            List<Transaction> transactions = FetchUnsyncDataTransaction();
            List<TransactionDetail> transactionDetails = FetchUnsyncDataTransactionDetail();

            if (transactions.Count == 0 && transactionDetails.Count == 0)
            {
                Console.WriteLine(ConstantVariable.SYNC_UP_TO_DATE);
                return;
            }

            if (transactions.Count > 0 && TransactionIds.Count > 0)
            {
                try
                {
                    // Sync Data Table 'deduct_card_results'
                    foreach (Transaction transaction in transactions)
                    {
                        int? parkingOutId = transaction.ParkingOutId;
                        int? parkingInId = transaction.ParkingInId;
                        string result = transaction.DeductResult;
                        int amount = transaction.Amount;
                        string transactionDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(transaction.TransactionDatetime);
                        string created = TKHelper.ConvertDatetimeToDefaultFormatMySQL(transaction.CreatedDatetime);
                        string bank = transaction.Bank;
                        string ipv4 = transaction.IpAddress;
                        string operatorName = transaction.OperatorName;
                        string idReader = transaction.IdReader;

                        string tableName = "deduct_card_results";
                        string query = string.Format("INSERT INTO {0} (parking_out_id, parking_in_id, result, amount, transaction_dt, bank, ipv4, operator, ID_reader, created)" +
                                       "VALUES(@parking_out_id, @parking_in_id, @result, @amount, @transaction_dt, @bank, @ipv4, @operator, @ID_reader, @created)", tableName);
                        Dictionary<string, object> param = new Dictionary<string, object>()
                        {
                            {"@parking_out_id", parkingOutId ?? (object)DBNull.Value },
                            {"@parking_in_id", parkingInId ?? (object)DBNull.Value },
                            {"@result", result },
                            {"@amount", amount },
                            {"@transaction_dt", transactionDt },
                            {"@bank", bank },
                            {"@ipv4", ipv4 },
                            {"@operator", operatorName },
                            {"@ID_reader", idReader },
                            {"@created", created }
                        };
                        Insert("server", query, param);
                    }

                    foreach (int transactionId in TransactionIds)
                    {
                        string query = "update deduct_card_results set has_synced = 1 where id = " + transactionId;
                        Update("local", query);
                    }

                    string resultSuccess = String.Format("{0} Transaction {1}", TransactionIds.Count, ConstantVariable.SYNC_DATA_SUCCESS);
                    Console.WriteLine(resultSuccess);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INSERT_TRANSACTION_RECORD_INTO_DATABASE + "\nError : " + ex.Message);
                }
            }

            // Sync Data table 'deduct_card_result_details'
            if (transactionDetails.Count > 0 && TransactionDetailIds.Count > 0)
            {
                try
                {
                    foreach (TransactionDetail transactionDetail in transactionDetails)
                    {
                        int deductCardResultId = transactionDetail.DeductCardResultId;
                        int? peopleTicketId = transactionDetail.PeopleTicketId;
                        int? cargoFareId = transactionDetail.CargoFareId;
                        int amount = transactionDetail.Amount;
                        string created = transactionDetail.Created;

                        string databaseName = "deduct_card_result_details";
                        string query = string.Format("INSERT INTO {0} (deduct_card_result_id, people_ticket_id, cargo_fare_id, amount, created) " +
                                       "VALUES(@deduct_card_result_id, @people_ticket_id, @cargo_fare_id, @amount, @created)", databaseName);

                        Dictionary<string, object> param = new Dictionary<string, object>()
                        {
                            { "@deduct_card_result_id", deductCardResultId },
                            { "@people_ticket_id", peopleTicketId ?? (object)DBNull.Value},
                            { "@cargo_fare_id", cargoFareId ?? (object)DBNull.Value },
                            { "@amount", amount},
                            { "@created", created}
                        };
                        Insert("server", query, param);
                    }

                    foreach (int transactionDetailId in TransactionDetailIds)
                    {
                        string query = "update deduct_card_result_details set has_synced = 1 where id = " + transactionDetailId;
                        Update("local", query);
                    }

                    string resultSuccess = String.Format("{0} Transaction Detail {1}", TransactionDetailIds.Count, ConstantVariable.SYNC_DATA_SUCCESS);
                    Console.WriteLine(resultSuccess);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INSERT_TRANSACTION_RECORD_INTO_DATABASE + "\nError : " + ex.Message);
                }
            }
        }
    }
}
