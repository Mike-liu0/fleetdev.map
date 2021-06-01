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

public partial class OnRequestLoginByMobile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        Sycamore_User user = new Sycamore_User();    // New user
        AWSController aws = new AWSController();

        // API input parameter
        string key = Request["key"];
        string mobile = Request["mobile"];
        string code = Request["code"];
        string token = Request["token"];   // TODO same name with data struct




        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "API Key Authentication fail", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if (mobile == null || mobile == "" || code == "" || code == null || token == "" || token == null)    // Check mobile number
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Data Input Incorrect", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }


        // (1) If user by accesstoken to ensure that user didn't change a device to login
        // (2) Check if user change mobile number
        // (3) Check if securityCode expires
        // (4) Check if securityCode send by user match with database
        try
        {
            List<Document> result = aws.OnDynamoDB_GetAllUserDocuments("accesstoken", token);
            string resultDoc = "";

            if (result.Count != 1)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Token Wrong, please try again", Sycamore_User.ToJson(user));
                Response.Write(response.ToString());
                return;
            }

            resultDoc = result[0].ToJson();
            user = Sycamore_User.FromJson(resultDoc);


            // Check if user change mobile number
            // Check if securityCode and mobile number match
            // Check if security code expires
            if (mobile != user.mobile)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "The mobile number is Incorrect!", Sycamore_User.ToJson(new Sycamore_User()));
                Response.Write(response.ToString());
                return;
            }

            if(code != user.securityCode)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "The security code is Incorrect", Sycamore_User.ToJson(new Sycamore_User()));
                Response.Write(response.ToString());
                return;
            }

            if (user.IsCodeExpired())
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "The security code is expired, please send sms again", Sycamore_User.ToJson(new Sycamore_User()));
                Response.Write(response.ToString());
                return;
            }



            // Login successfully
            user.RenewToken();
            string updateResult = aws.OnDynamoDB_ModifyUser(user);    // Make sure update user info successfully
            if (updateResult != user.guid)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Update User Info Error ", Sycamore_User.ToJson(user));
                Response.Write(response.ToString());
                return;
            }

            // Login Successfully
            response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}", "Sign in complete", Sycamore_User.ToJson(user));
            // Add system log
            SystemLog log = new SystemLog(key, "OnRequestLoginByMobile.aspx", "Login Successfully", "True");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", ex.Message, ex.Message);
            // System Log
            SystemLog log = new SystemLog(key, "OnUserRequestAuthCode.aspx", "request exception ", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            Response.Write(response.ToString());
            return;
        }
        Response.Write(response.ToString());
        return;
    }
}
