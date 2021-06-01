using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Notification
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Notification
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Notification";

        // Must have
        public string target = "Default_Target";
        public Sycamore_Notification()
        {
           
        }
        public static string ToJson(Sycamore_Notification _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Notification FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Notification>(_json);
        }
    }
}