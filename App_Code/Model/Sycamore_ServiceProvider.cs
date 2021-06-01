using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ServiceProvider
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_ServiceProvider
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_ServiceProvider";

        // Must have
        public string serviceProvider = "Default_Provider_Name";
        public Sycamore_ServiceProvider()
        {

        }
        public static string ToJson(Sycamore_ServiceProvider _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_ServiceProvider FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_ServiceProvider>(_json);
        }
    }
}