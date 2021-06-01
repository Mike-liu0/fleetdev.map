using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spire.Xls;

namespace Darkspede
{

    public class LogController
    {
        public static void WriteToLog(string path, SystemLog log)
        {
            try
            {
                var t = new Task(() => {
                    DateTime today = DateTime.Today;
                    var fileName = today.ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv";

                    if (!File.Exists(path + fileName))
                    {
                        File.WriteAllText(path + fileName, "guid,localTime,created,updated,status,attachedUser,path,apiTarget,actionTarget,success,resultDescription\n");
                    }

                    using (StreamWriter sw = File.AppendText(path + fileName))
                    {
                        sw.WriteLine(SystemLog.ToCsvLine(log));
                    }
                } );

                t.Start();
                
            }
            catch
            {

            }
        }

        public string CheckBugs(string path, string time)
        {
            List<string> filesToSearch = new List<string>();
            if (time == "today")
            {
                filesToSearch.Add(DateTime.Today.ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv");
            }
            else if (time == "a week")
            {
                int i = 0;
                while (i < 7)
                {
                    filesToSearch.Add(DateTime.Today.AddDays(-i).ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv");
                    i++;
                }
            }
            else if (time == "a month")
            {
                int i = 0;
                while (i < 30)
                {
                    filesToSearch.Add(DateTime.Today.AddDays(-i).ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv");
                    i++;
                }
            }

            string result = "";
            foreach (string file in filesToSearch)
            {
                if (File.Exists(path + file))
                {
                    Workbook wb = new Workbook();
                    wb.LoadFromFile(path + file, ",", 1, 1);
                    Worksheet ws = wb.Worksheets[0];
                    int row = 1;
                    while (row <= ws.LastRow)
                    {
                        if (ws.Range["J" + row].Text == "false" || ws.Range["J" + row].Text == "FALSE")
                        {
                            if (result != "")
                            {
                                result += "|";
                            }
                            // Remove special characters to prevent bugs
                            string logTime = ws.Range["B" + row].Text.Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\'", "").Replace("\\", "\\\\");
                            string logAPIName = ws.Range["H" + row].Text.Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\'", "").Replace("\\", "\\\\");
                            string logAction = ws.Range["I" + row].Text.Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\'", "").Replace("\\", "\\\\");
                            string logDescription = ws.Range["K" + row].Text.Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\'", "").Replace("\\", "\\\\");

                            string thisLog = logTime + "," + logAPIName + "," + logAction + "," + logDescription;

                            result += thisLog;
                        }
                        row++;
                    }
                }
            }
            return result;
        }

        public string CheckLatestLog(string path)
        {
            bool fileFound = false;
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                if (files.Length != 0)
                {
                    string fileName = "";
                    int i = 0;
                    while (!fileFound)
                    {
                        if(File.Exists(path + DateTime.Today.AddDays(-i).ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv"))
                        {
                            fileName = DateTime.Today.AddDays(-i).ToString(Config.LogFileNameDateFormat) + "_SystemLog.csv";
                            fileFound = true;
                        }
                        i++;
                    }

                    Workbook wb = new Workbook();
                    wb.LoadFromFile(path + fileName, ",", 1, 1);
                    Worksheet ws = wb.Worksheets[0];
                    int row = ws.LastRow;
                    string logTime = ws.Range["B" + row].Text.Replace("{", "").Replace("}", "").Replace(",", "").Replace("\"", "").Replace("\'", "").Replace("\\", "\\\\");
                    return logTime;

                }
                else
                {
                    return "No log files available.";
                }
            }
            else
            {
                return "No log files available.";
            }
        }
    }
}