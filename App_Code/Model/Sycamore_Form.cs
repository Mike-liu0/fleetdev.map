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
    public class Sycamore_Form
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Form";

        public string formTitle;
        public string formName;
        public string formOrginzation;




        public Sycamore_Form()
        {

        }
        public static string ToJson(Sycamore_Form _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Form FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Form>(_json);
        }
    }
}