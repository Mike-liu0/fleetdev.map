using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;




namespace Darkspede { 
    [Serializable]
    public class SystemLog
    {
        public string guid = Guid.NewGuid().ToString();
        public string localTime = DateTime.Now.ToString(Config.LogDateTimeFormat);
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string path = Config.DevelopmentStage;

        public string attachedUser;
        public string targetApi;
        public string targetAction;
        public string success;
        public string resultDescription = "default";

        public SystemLog(string attachedUser, string targetApi, string targetAction, string success, string resultDescription="default")
        {
            if(resultDescription == "")
            {
                resultDescription = "default";
            }
            this.attachedUser = attachedUser;
            this.targetApi = targetApi;
            this.targetAction = targetAction;
            this.success = success;
            this.resultDescription = resultDescription;
        }

        public static string ToJson(SystemLog _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static SystemLog FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<SystemLog>(_json);
        }

        public static string ToCsvLine(SystemLog _item)
        {
            var data = _item.guid + "," + _item.localTime + "," + _item.created + "," + _item.updated + "," + _item.status + "," + _item.attachedUser + "," + _item.path + "," + _item.targetApi + "," + _item.targetAction + "," + _item.success + "," + _item.resultDescription;
            return data;
        }
    }
}
