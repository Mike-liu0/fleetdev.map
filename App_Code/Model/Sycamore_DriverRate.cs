using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DriverRate
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_DriverRate
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_DriverRate";
        public Sycamore_DriverRate()
        {
          
        }
        public static string ToJson(Sycamore_DriverRate _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_DriverRate FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_DriverRate>(_json);
        }
    }
}