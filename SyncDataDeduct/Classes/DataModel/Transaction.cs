using Newtonsoft.Json;

namespace SyncDataDeduct.Classes.DataModel
{
    class Transaction
    {
        [JsonProperty("id")]
        public int TransactionId { get; set; }

        [JsonProperty("parking_out_id")]
        public int? ParkingOutId { get; set; }

        [JsonProperty("parking_in_id")]
        public int? ParkingInId { get; set; }

        [JsonProperty("result")]
        public string DeductResult { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("transaction_dt")]
        public string TransactionDatetime { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("ipv4")]
        public string IpAddress { get; set; }

        [JsonProperty("operator")]
        public string OperatorName { get; set; }

        [JsonProperty("ID_reader")]
        public string IdReader { get; set; }

        [JsonProperty("has_synced")]
        public int HasSynced { get; set; }

        [JsonProperty("created")]
        public string CreatedDatetime { get; set; }

        public Transaction(int transactionId, int? parkingOutId, int? parkingInId, string result, int amount, string transactionDt, string bank, string ipv4, string operatorName, string idReader, int hasSynced, string created)
        {
            TransactionId = transactionId;
            ParkingOutId = parkingOutId;
            ParkingInId = parkingInId;
            DeductResult = result;
            Amount = amount;
            TransactionDatetime = transactionDt;
            Bank = bank;
            IpAddress = ipv4;
            OperatorName = operatorName;
            IdReader = idReader;
            HasSynced = hasSynced;
            CreatedDatetime = created;
        }
    }
}
