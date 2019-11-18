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

        public long Insert(string type, string query_cmd)
        {
            string query = query_cmd;
            long insert_last_id = -1;
            if (this.OpenConnection(type))
            {
                MySqlCommand cmd = type == "local" ? new MySqlCommand(query, localConnection) : new MySqlCommand(query, serverConnection);
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

                    int parkingOutId = Convert.ToInt32(dataReader["parking_out_id"].ToString());
                    string deductResult = dataReader["result"].ToString();
                    string transactionDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(dataReader["transaction_dt"].ToString());
                    int amount = Convert.ToInt32(dataReader["amount"].ToString());
                    string ipv4 = dataReader["ipv4"].ToString();
                    string operatorName = dataReader["operator"].ToString();
                    string idReader = dataReader["ID_reader"].ToString();
                    string bank = dataReader["bank"].ToString();
                    int hasSynced = Convert.ToInt32(dataReader["has_synced"].ToString());
                    string created = TKHelper.ConvertDatetimeToDefaultFormatMySQL(dataReader["created"].ToString());
                    transactions.Add(new Transaction(id, parkingOutId, deductResult, amount, transactionDt, bank, ipv4, operatorName, idReader, hasSynced, created));
                }
                CloseConnection("local");
            }
            return transactions;
        }

        public void SyncDataToServer()
        {
            List<Transaction> transactions = FetchUnsyncDataTransaction();
            if (transactions.Count > 0 && TransactionIds.Count > 0)
            {
                try
                {
                    foreach (Transaction transaction in transactions)
                    {
                        int parkingOutId = transaction.ParkingOutId;
                        string result = transaction.DeductResult;
                        int amount = transaction.Amount;
                        string transactionDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(transaction.TransactionDatetime);
                        string created = TKHelper.ConvertDatetimeToDefaultFormatMySQL(transaction.CreatedDatetime);
                        string bank = transaction.Bank;
                        string ipv4 = transaction.IpAddress;
                        string operatorName = transaction.OperatorName;
                        string idReader = transaction.IdReader;

                        string query = "INSERT INTO deduct_card_results (parking_out_id, result, amount, transaction_dt, bank, ipv4, operator, ID_reader, created) VALUES('" + parkingOutId + "', '" +
                        result + "', '" + amount + "', '" + transactionDt + "', '" + bank + "', '" + ipv4 + "', '" + operatorName + "', '" + idReader + "', '" + created + "')";

                        Insert("server", query);
                    }

                    foreach (int transactionId in TransactionIds)
                    {
                        string query = "update deduct_card_results set has_synced = 1 where id = " + transactionId;
                        Update("local", query);
                    }
                    Console.WriteLine(TransactionIds.Count + ConstantVariable.SYNC_DATA_SUCCESS);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INSERT_TRANSACTION_RECORD_INTO_DATABASE + "\nError : " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine(ConstantVariable.SYNC_UP_TO_DATE);
            }
        }
    }
}
