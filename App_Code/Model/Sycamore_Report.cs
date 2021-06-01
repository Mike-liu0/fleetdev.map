using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Report
/// </summary>
/// 
namespace Darkspede
{
    public class Sycamore_Report
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Report";

        public string attachedUser;  // *
        public string attachedPost;  // *
        public string reportType = "general";  
        public string reportPlatform = "app";
        public string reportContent = "Default";
        public string processResult = "Unprocessed";

        public Sycamore_Report()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public static string ToJson(Sycamore_Report _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Report FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Report>(_json);
        }

    }
}