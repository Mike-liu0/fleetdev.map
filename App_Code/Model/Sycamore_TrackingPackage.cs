using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_TrackingPackage
/// </summary>


namespace Darkspede
{
    [Serializable]
    public class Sycamore_TrackingPackage
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_TrackingPackage";

        // Must have
        public string position = "Default_Position";
        public string engineState = "Default_State";
        public string obdDataPack = "Default_Data_Pack";
        public Sycamore_TrackingPackage()
        {

        }
        public static string ToJson(Sycamore_TrackingPackage _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_TrackingPackage FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_TrackingPackage>(_json);
        }
    }
}