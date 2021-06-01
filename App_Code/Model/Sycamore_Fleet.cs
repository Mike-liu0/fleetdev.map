using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Fleet
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Fleet
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Fleet";

        // Must have
        public  Queue<Sycamore_Vehicle> vchicles;
        public Sycamore_Fleet()
        {
            vchicles = new Queue<Sycamore_Vehicle>();
        }
        public static string ToJson(Sycamore_Fleet _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Fleet FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Fleet>(_json);
        }
    }
}