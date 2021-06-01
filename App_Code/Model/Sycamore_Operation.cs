using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Operation
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Operation
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Operation";
        public string operationCmd = "0000";

        public Queue<Sycamore_TrackingRecord> trackingRecords;

        public Sycamore_Operation()
        {
            trackingRecords = new Queue<Sycamore_TrackingRecord>();
        }

        public static string ToJson(Sycamore_Operation _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Operation FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Operation>(_json);
        }
    }
}