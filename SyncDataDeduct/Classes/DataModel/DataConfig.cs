using Newtonsoft.Json;

namespace SyncDataDeduct.Classes.DataModel
{
    class DataConfig
    {
        [JsonProperty("server_database_host")]
        public static string ServerDBHost { get; set; }

        [JsonProperty("server_database_name")]
        public static string ServerDBName { get; set; }

        [JsonProperty("server_database_username")]
        public static string ServerDBUsername { get; set; }

        [JsonProperty("server_database_password")]
        public static string ServerDBPassword { get; set; }

        [JsonProperty("local_database_host")]
        public static string LocalDBHost { get; set; }

        [JsonProperty("local_database_name")]
        public static string LocalDBName { get; set; }

        [JsonProperty("local_database_username")]
        public static string LocalDBUsername { get; set; }

        [JsonProperty("local_database_password")]
        public static string LocalDBPassword { get; set; }

        [JsonProperty("limit_sync_data")]
        public static int LimitSyncData { get; set; }
    }
}