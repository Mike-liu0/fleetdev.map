using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace Darkspede
{
    [Serializable]
    public class Sycamore_User
    {
        public string guid = Guid.NewGuid().ToString();
        public string created = DateTime.Now.Ticks.ToString();
        public string updated = DateTime.Now.Ticks.ToString();
        public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
        public string tableName = "Sycamore_User";

        // Must have
        public string mobile = "614000000";
        public string accesstoken =Guid.NewGuid().ToString();
        public string tokenExpire = DateTime.Now.Ticks.ToString();

        public string securityCode= Guid.NewGuid().ToString();
        public string codeExpire = DateTime.Now.Ticks.ToString();


        // Update Later
        public string username = "N";
        public string fullName = "Default User";
        public string email = "N";
        public string password = "N"; // stored as SHA
        public string location = "Melbourne";
        public string companyName = "N";
        public string companyType = "Not Stated";

        public string permission = "C1"; // Check the permission table for permitted operation of each permission group
        public string iconUrl = "default";
        public string fcmToken = "";

        public Sycamore_User()
        {

        }

        public string RenewCode()
        {
            if (!Config.CheckUserException(mobile))
            {
                securityCode = Utility.GenerateSecurityCode();
            }

            codeExpire = DateTime.Now.AddMinutes(Config.CodeExpire).Ticks.ToString();
            return accesstoken;
        }

        public bool IsCodeExpired()
        {
            if (DateTime.Now.Ticks < long.Parse(codeExpire))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string RenewToken()
        {
            accesstoken = Guid.NewGuid().ToString();
            tokenExpire = DateTime.Now.AddDays(Config.TokenExpire).Ticks.ToString();
            return accesstoken;
        }

        public bool IsTokenExpired()
        {
            if(DateTime.Now.Ticks < long.Parse(tokenExpire))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string ToJson(Sycamore_User _item)
        {
            return JsonConvert.SerializeObject(_item);
        }

        public static Sycamore_User FromJson(string _json)
        {
            return JsonConvert.DeserializeObject<Sycamore_User>(_json);
        }
    }
}