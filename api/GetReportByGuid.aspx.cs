using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede;

// API Object [Darkspede_User]
public partial class GetReportByGuid : System.Web.UI.Page
{
    public static string OUTPUT = Sycamore_Report.ToJson(new Sycamore_Report());
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();

        string key = Request["key"];
        string output = Request["output"];
        string guid = Request["guid"];

        Sycamore_Report package_item = new Sycamore_Report();

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", Sycamore_Report.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        if (output == "output")
        {
            response.AppendFormat("{0}", Sycamore_Report.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if (guid == "" || guid == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Data input fail", Sycamore_Report.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }
        try
        {
            string result = new AWSController().OnDynamoDB_GetReportByGuid(guid);

            if(result != "none")
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                    "API.GetReportByGuid Complete", result);

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else{
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                    "Database Error: " + result, Sycamore_Report.ToJson(package_item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }

        }
        catch(Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "API Error: " + ex.Message, Sycamore_Report.ToJson(package_item));

            // System Log
            SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;


    }
}
