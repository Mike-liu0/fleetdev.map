using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Document
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Document
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Document";

        // Must have
        public string documentType = "Default_Type";
        public Sycamore_Document()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public static string ToJson(Sycamore_Document _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Document FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Document>(_json);
        }
    }
}