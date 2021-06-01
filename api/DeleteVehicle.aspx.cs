/* Delete user by guid */
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using Newtonsoft.Json;
using Darkspede;

// API Object [Darkspede_User]
public partial class DeleteVehicle : System.Web.UI.Page
{
    public static string OUTPUT = Sycamore_Vehicle.ToJson(new Sycamore_Vehicle());
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string output = Request["output"];
        string guid = Request["guid"];

        Sycamore_Vehicle package_item = new Sycamore_Vehicle();

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", Sycamore_Vehicle.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        if (output != null)
        {
            response.AppendFormat("{0}", Sycamore_Vehicle.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if (guid == "" || guid == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                 "Data input fail", Sycamore_Vehicle.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }


        try
        {
            string result = new AWSController().OnDynamoDB_DeleteVehicleByGuid(guid);

            if (result == "success")
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                    "API.DeleteVehicleByGuid  Complete", Sycamore_Vehicle.ToJson(package_item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS delete item", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                    "Database Error: " + result, Sycamore_Vehicle.ToJson(package_item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS delete item", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }


            // update the rest

        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\"}}", "API Error: " + ex.Message);

            // System Log
            SystemLog log = new SystemLog(key, this.GetType().Name, "AWS delete item", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}
