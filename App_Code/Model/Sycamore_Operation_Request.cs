using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Operation_Request
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Operation_Request
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Operation_Request";
        public string operationCmd = "0000";

        public Queue<Sycamore_Operation> Operations;

        public Sycamore_Operation_Request()
        {
            Operations = new Queue<Sycamore_Operation>();
        }

        public static string ToJson(Sycamore_Operation_Request _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Operation_Request FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Operation_Request>(_json);
        }
    }
}