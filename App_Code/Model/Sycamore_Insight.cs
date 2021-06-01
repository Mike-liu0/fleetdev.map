using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Insight
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Insight
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Insight";

        // Must have
        public string insightType = "Default_Type";
        public Sycamore_Insight()
        {

        }
        public static string ToJson(Sycamore_Insight _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Insight FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Insight>(_json);
        }
    }
}