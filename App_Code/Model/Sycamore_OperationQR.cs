using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for OperationQR
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_OperationQR
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "OperationQR";

        // Must have
        public string deviceID = "Default_ID";
        public Sycamore_OperationQR()
        {
         
        }
        public static string ToJson(Sycamore_OperationQR _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_OperationQR FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_OperationQR>(_json);
        }
    }
}