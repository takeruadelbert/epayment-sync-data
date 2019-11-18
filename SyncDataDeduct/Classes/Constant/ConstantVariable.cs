namespace SyncDataDeduct.Classes.Constant
{
    class ConstantVariable
    {
        public static readonly string DATE_FORMAT_IN_WORD = "dd MMMM yyyy";
        public static readonly string DATE_FORMAT_DEFAULT = "yyyy-MM-dd";
        public static readonly string TIME_FORMAT_DEFAULT = "HH:mm:ss";
        public static readonly string DATETIME_FORMAT_DEFAULT = "yyyy-MM-dd HH:mm:ss";

        public static readonly string DIR_PATH_CONFIG_FILE = @"\Classes\Configuration\config.json";
        
        public static readonly string SYNC_UP_TO_DATE = "Data is up to date. No sync needed.";
        public static readonly string SYNC_DATA_SUCCESS = " Data has successfully been synced to server!";

        public static readonly string ERROR_MESSAGE_FAIL_TO_PARSE_FILE_CONFIG = "Error occurred while parsing configuration file.";
        public static readonly string ERROR_MESSAGE_CANNOT_ESTABLISH_CONNECTION_TO_SERVER = "Cannot connect to server.  Contact administrator.";
        public static readonly string ERROR_MESSAGE_INVALID_USERNAME_PASSWORD = "Invalid username/password, please try again.";
        public static readonly string ERROR_MESSAGE_UNABLE_TO_BACKUP = "Error , unable to backup!";
        public static readonly string ERROR_MESSAGE_UNABLE_TO_RESTORE = "Error , unable to Restore!";
        public static readonly string ERROR_MESSAGE_INSERT_SETTLEMENT_RECORD_INTO_DATABASE = "Error : something's wrong while inserting new record of settlement file.";
        public static readonly string ERROR_MESSAGE_INSERT_TRANSACTION_RECORD_INTO_DATABASE = "Error : fail to insert data.";
    }
}
