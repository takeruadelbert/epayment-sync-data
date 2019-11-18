using Newtonsoft.Json;
using SyncDataDeduct.Classes.DataModel;
using System;
using System.IO;
using SyncDataDeduct.Classes.Constant;

namespace SyncDataDeduct.Classes.Helper
{
    class TKHelper
    {
        public static string GetApplicationExecutableDirectoryName()
        {
            string workingDirectory = Environment.CurrentDirectory;
            return Directory.GetParent(workingDirectory).Parent.Parent.FullName;
        }

        public static DataConfig ParseDataConfig(string configFile = "")
        {
            configFile = string.IsNullOrEmpty(configFile) ? GetApplicationExecutableDirectoryName() + ConstantVariable.DIR_PATH_CONFIG_FILE : configFile;
            using (StreamReader r = new StreamReader(configFile))
            {
                string json = r.ReadToEnd();
                DataConfig config = JsonConvert.DeserializeObject<DataConfig>(json);
                return config;
            }
        }

        // Default Format : yyyy-MM-dd HH:mm:ss
        public static string ConvertDatetimeToDefaultFormat(string dt)
        {
            string[] temp = dt.Split(' ');
            string date = temp[0];
            string month = GetMonthInNumber(temp[1]);
            string year = temp[2];
            string time = temp[3];
            return year + "-" + month + "-" + date + " " + time;
        }

        // Format : dd/MM/yyyy HH:mm:ss
        public static string ConvertDatetimeToDefaultFormatMySQL(string dt)
        {
            DateTime dateTime = DateTime.Parse(dt);
            string result = dateTime.ToString(ConstantVariable.DATETIME_FORMAT_DEFAULT);
            return result;
        }

        private static string GetMonthInNumber(string month, int digit = 2)
        {
            int month_in_number = -1;
            switch (month)
            {
                case "Januari":
                    month_in_number = 1;
                    break;
                case "Februari":
                    month_in_number = 2;
                    break;
                case "Maret":
                    month_in_number = 3;
                    break;
                case "April":
                    month_in_number = 4;
                    break;
                case "Mei":
                    month_in_number = 5;
                    break;
                case "Juni":
                    month_in_number = 6;
                    break;
                case "Juli":
                    month_in_number = 7;
                    break;
                case "Agustus":
                    month_in_number = 8;
                    break;
                case "September":
                    month_in_number = 9;
                    break;
                case "Oktober":
                    month_in_number = 10;
                    break;
                case "November":
                    month_in_number = 11;
                    break;
                case "Desember":
                    month_in_number = 12;
                    break;
                default:
                    month_in_number = -1;
                    break;
            }
            return month_in_number != -1 ? month_in_number.ToString("00") : "";
        }
    }
}