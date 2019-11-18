using SyncDataDeduct.Classes.Constant;
using SyncDataDeduct.Classes.DataModel;
using SyncDataDeduct.Classes.DB;
using SyncDataDeduct.Classes.Helper;
using System;

namespace SyncDataDeduct
{
    class Program
    {
        static void Main(string[] args)
        {
            DataConfig dataConfig = TKHelper.ParseDataConfig();
            if (dataConfig != null)
            {
                Database database = new Database();
                database.SyncDataToServer();
            }
            else
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_FAIL_TO_PARSE_FILE_CONFIG);
            }
            Console.ReadLine();
        }
    }
}
