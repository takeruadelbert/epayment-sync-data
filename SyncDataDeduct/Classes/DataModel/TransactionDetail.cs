using Newtonsoft.Json;

namespace SyncDataDeduct.Classes.DataModel
{
    class TransactionDetail
    {
        [JsonProperty("id")]
        public int TransactionDetailId { get; set; }

        [JsonProperty("deduct_card_result_id")]
        public int DeductCardResultId { get; set; }

        [JsonProperty("people_ticket_id")]
        public int? PeopleTicketId { get; set; }

        [JsonProperty("cargo_fare_id")]
        public int? CargoFareId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("has_synced")]
        public int HasSynced { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        public TransactionDetail(int transactionDetailId, int deductCardResultId, int? peopleTicketId, int? cargoFareId, int amount, int hasSynced, string created)
        {
            TransactionDetailId = transactionDetailId;
            DeductCardResultId = deductCardResultId;
            PeopleTicketId = peopleTicketId;
            CargoFareId = cargoFareId;
            Amount = amount;
            HasSynced = hasSynced;
            Created = created;
        }
    }
}
