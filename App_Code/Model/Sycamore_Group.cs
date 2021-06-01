using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Group
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Group
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Group";

        // Must have
        public string attachedVehicle = "Default_Vehicle";
        public Sycamore_Group()
        {
   
        }
        public static string ToJson(Sycamore_Group _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Group FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Group>(_json);
        }
    }
}