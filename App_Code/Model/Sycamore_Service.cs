using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Service
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Service
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Service";

        // Must have
        public string attachedServiceProvider = "Default_Provider";
        public string attachedDocument = "Default_Document";
        public Sycamore_Service()
        {

        }
        public static string ToJson(Sycamore_Service _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Service FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Service>(_json);
        }
    }
}