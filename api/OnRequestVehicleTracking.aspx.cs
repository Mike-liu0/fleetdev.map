using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using Darkspede;

//https://fleetdev.allinks.com.au/api/OnRequestVehicleTracking.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&deviceID=D000-0001
public partial class OnRequestVehicleTracking : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string deviceID = Request["deviceID"];

        string OUTPUT = Sycamore_Operation.ToJson(new Sycamore_Operation());


        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", OUTPUT);
            Response.Write(response.ToString());
            return;
        }


        try
        {
            Sycamore_Operation operation = TrackingController.Instance.GetOperation(deviceID);

            if (operation == null)
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                               "API.OnRequestVehicleTracking", "{}");
            }
            else
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                  "API.OnRequestVehicleTracking: ", Sycamore_Operation.ToJson(operation));
            }


            SystemLog log = new SystemLog(key, "OnRequestVehicleTracking.aspx", "Request Tracking Data", "true", OUTPUT);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                            "API Error: " + ex.Message, OUTPUT);
            // System Log
            SystemLog log = new SystemLog(key, "OnSendVehicleTracking.aspx", "Record Tracking Data", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}