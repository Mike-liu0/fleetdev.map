using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede;
using Amazon.DynamoDBv2.DocumentModel;
using System.Net;


//Use SMS to auth user

public partial class OnRequestAuthCodeByMobile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();

        Sycamore_User user = new Sycamore_User();    // New user
        AWSController aws = new AWSController();    // New Controller

        string key = Request["key"];
        string mobile = Request["mobile"];


        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " API Key Error! Authentication fail", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }

        // Mobile input check: return none if there is no mobile input
        if (mobile == "" || mobile == null)    // No mobile input
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Please Input Mobile Number", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }

        // Valiate mobile input format.
        // transfer 04 & 4 & 61 -> 61, region is "aus"
        string newMobile = Utility.ValidateMobile(mobile, "aus");


        // If user input mobile is invalid
        if (newMobile == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Invalid Mobile Format", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }


        // Mobile is valid and wait for parsing
        try
        {
            //returned list with user document
            List<Document> result = aws.OnDynamoDB_GetAllUserDocuments("mobile", newMobile);    
            string resultDoc = "";    // get list document with index 0

            if (result.Count == 0)
            {
                // User doesn't exist, create a new user, and pass newMobile to this user
                user.mobile = newMobile;
                aws.OnDynamoDB_AddNewUser(user);    // Add new user in DB
            }
            else if(result.Count == 1)
            {
                resultDoc = result[0].ToJson();    //Transfer list to json
                user = Sycamore_User.FromJson(resultDoc);
            }
            else    // if count > 1
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Database Error! ", Sycamore_User.ToJson(user));
                Response.Write(response.ToString());
                return;
            }


            // Refresh securityCode expire date and token expire date
            user.RenewCode();
            user.RenewToken();
            user.updated = DateTime.Now.Ticks.ToString();

            string updateResult = aws.OnDynamoDB_ModifyUser(user);    // Make sure update user info successfully
            if(updateResult != user.guid)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Update User Info Error ", Sycamore_User.ToJson(user));
                Response.Write(response.ToString());
                return;
            }


            // Call OnRequestSendSMS api                        
            string security_codeString = user.securityCode + Config.AUTH_CODE_SURFIX;
            string link = Config.smsLink + newMobile + "&message=" + security_codeString;
            WebRequest request = WebRequest.Create(link);
            request.GetResponse();


            // Hide user security code in production stage
            if (Config.IsDebugEnabled)
            {
                user.securityCode = Config.securityCode;
            }

            // return user document with new security code 
            response.AppendFormat("{{\"success\":\"true\",\"message\":\"{0}\", \"package\":{1}}}", "Code Sent", Sycamore_User.ToJson(user));

            // System Log
            SystemLog log = new SystemLog(key, "OnUserRequestAuthCode.aspx", "user request authorize code", "true");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);

        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "API Error: ", ex.Message);

            // System Log
            SystemLog log = new SystemLog(key, "OnUserRequestAuthCode.aspx", "user request authorize code", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}
