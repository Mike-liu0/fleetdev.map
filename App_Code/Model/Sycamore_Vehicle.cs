using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Sycamore_Vehicle
/// </summary>
namespace Darkspede
{
    [Serializable]
    public class Sycamore_Vehicle
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_Vehicle";

        // Must have
        public Queue<Sycamore_Operation> operations;
        public Queue<Sycamore_Document> documents;
        public Queue<Sycamore_Service> services;
        public Sycamore_Vehicle()
        {
            operations = new Queue<Sycamore_Operation>();
            documents = new Queue<Sycamore_Document>();
            services = new Queue<Sycamore_Service>();
        }
        public static string ToJson(Sycamore_Vehicle _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_Vehicle FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_Vehicle>(_json);
        }
    }
}