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

public partial class OnRequestLoginByToken : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();

        Sycamore_User user = new Sycamore_User();    // New user
        AWSController aws = new AWSController();

        // API input parameter
        string key = Request["key"];
        string token = Request["token"];


        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "API Key Authentication fail", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if(token == null || token == "")    // Check token
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Please input token", Sycamore_User.ToJson(user));
            Response.Write(response.ToString());
            return;
        }


        // Find token in DB, if it is in DB and not expired, login successfully, or login failed
        try
        {
            List<Document> result = aws.OnDynamoDB_GetAllUserDocuments("accesstoken", token);
            
            if (result.Count != 1)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Token doesn't exist, please try again", Sycamore_User.ToJson(user));
                Response.Write(response.ToString());
                return;
            }

            // Token exist
            user = Sycamore_User.FromJson( result[0].ToJson());

            // Check if token expired
            if (user.IsTokenExpired())
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Token expired, please use SMS to login", Sycamore_User.ToJson(new Sycamore_User()));
                Response.Write(response.ToString());
                return;
            }

            // Login successfully
            user.RenewToken();    // Renew
            string updateResult = aws.OnDynamoDB_ModifyUser(user);    // Make sure update user info successfully
            if (updateResult != user.guid)
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", " Update User Info Error ", Sycamore_User.ToJson(new Sycamore_User()));
                Response.Write(response.ToString());
                return;
            }


            // Login successfully
            response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}", "Sign in complete", Sycamore_User.ToJson(user));
            // Write system log
            SystemLog log = new SystemLog(key, "OnRequestLoginByToken.aspx", "Login Successfully", "True");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Database Error: ", ex.Message);
            // System Log
            SystemLog log = new SystemLog(key, "OnUserRequestAuthCode.aspx", "user request authorize code", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            Response.Write(response.ToString());
            return;
        }
        Response.Write(response.ToString());
        return;
    }
}
