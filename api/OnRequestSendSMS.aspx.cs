using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede;
using Darkspede.Sycamore;


//https://dev.allinks.com.au/api/OnRequestSendSMS.aspx?key=jQTHqBgLA0aNSNoVUbU0NQ&target=61433280815
public partial class OnRequestSendSMS : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string target = Request["target"];
        string message = Request["message"];

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Authentication failed", FCM_SMS.ToJson(new FCM_SMS()));
            Response.Write(response.ToString());
            return;
        }


        
        try
        {
            string security_code = Utility.GenerateSecurityCode() + Config.AUTH_CODE_SURFIX;
            string sms = "";

            if(message == null || message == "")
            {
                sms = security_code;
            }
            else
            {
                sms = message;
            }



            if (!target.StartsWith("+"))
            {
                target = "+" + target;
            }


            string result = new AWSController().OnLambda_RequestSendSMS(sms, target);

            if (result != "")
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}", "API.RequestSendSMS Complete", result);

                // System Log
                SystemLog log = new SystemLog(key, "OnRequestSendSMS.aspx", "AWS request send sms", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "AWS Error: " + result, result);

                // System Log
                SystemLog log = new SystemLog(key, "OnRequestSendSMS.aspx", "AWS request send sms", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":\"{0}\"}}", "API Error: " + ex.Message);

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestSendSMS.aspx", "AWS request send sms", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }


        
        Response.Write(response.ToString());
        return;
    }
}
