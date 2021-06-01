using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using Darkspede;

// API Object [Darkspede_User]
public partial class GetAllTrackingRecords : System.Web.UI.Page
{
    public static string OUTPUT = Sycamore_TrackingRecord.ToJson(new Sycamore_TrackingRecord());
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();

        string key = Request["key"];
        string output = Request["output"];
        string tableName= Request["tableName"];
        string filterName = Request["filterName"];
        string filterValue = Request["filterValue"];

        Sycamore_TrackingRecord package_item = new Sycamore_TrackingRecord();


        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", Sycamore_TrackingRecord.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        if (output == "output")
        {
            response.AppendFormat("{0}", Sycamore_TrackingRecord.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if (filterName == "" || filterName == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                 "Data input fail", Sycamore_TrackingRecord.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }
        try
        {
            //the result is a list for object jsons
            string result = new AWSController().OnDynamoDB_GetAllTrackingRecords(tableName, filterName, filterValue);

            if (result != "[]")
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                     "API.GetAllTrackingRecords Complete", result);

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                            "Database Error: " + result, Sycamore_TrackingRecord.ToJson(package_item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }

        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\"}}", "API Error: " + ex.Message);

            // System Log
            SystemLog log = new SystemLog(key, this.GetType().Name, "AWS get item", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;


    }
}
