using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Org.BouncyCastle.Asn1.Cms;
using Amazon.DynamoDBv2.DocumentModel;
using Darkspede;

// API Object [Darkspede_User]
public partial class ModifyTrackingPackage : System.Web.UI.Page
{
    public static string OUTPUT = Sycamore_TrackingPackage.ToJson(new Sycamore_TrackingPackage());
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string output = Request["output"];
        string package = Request["package"];

        Sycamore_TrackingPackage package_item = new Sycamore_TrackingPackage();

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", Sycamore_TrackingPackage.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        if (output == "output")
        {
            response.AppendFormat("{0}", Sycamore_TrackingPackage.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }
        
        // Data input Check
        if (package == "" || package == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                 "Data input fail", Sycamore_TrackingPackage.ToJson(package_item));

            Response.Write(response.ToString());
            return;
        }

        // TODO
        // Checking for permission


        try
        {
            AWSController AWS = new AWSController();

            Sycamore_TrackingPackage item = Sycamore_TrackingPackage.FromJson(package);

            Guid guidResult;
            string result = new AWSController().OnDynamoDB_ModifyTrackingPackage(item);

            bool isValid = Guid.TryParse(result, out guidResult);

            if (isValid)
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                    "API.ModifyTrackingPackage Complete", Sycamore_TrackingPackage.ToJson(item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS modify item", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                    "Database Error: " + result, Sycamore_TrackingPackage.ToJson(item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS modify item", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "API Error: " + ex.Message, Sycamore_TrackingPackage.ToJson(package_item));

            // System Log
            SystemLog log = new SystemLog(key, this.GetType().Name, "AWS modify item", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}
