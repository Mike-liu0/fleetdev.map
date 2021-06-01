using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Driver
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Driver
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Driver";

        // Must have
        public Queue<Sycamore_Operation> operations;
        public Sycamore_Driver()
        {
            operations = new Queue<Sycamore_Operation>();
        }
        public static string ToJson(Sycamore_Driver _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Driver FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Driver>(_json);
        }
    }
}