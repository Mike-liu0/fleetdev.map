using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using Darkspede;
using System.Net;

//https://fleetdev.allinks.com.au/api/OnSendVehicleTrackingRandomly.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&package=1_D000-0001
//[packageID]_[deviceID]__[GPS STRING]
//1_D000-0001  _A_35_2000_D_E_F_G_H_I_J_K_GPS
public partial class OnSendVehicleTrackingRandomly : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string package = Request["package"];

        string OUTPUT = Sycamore_TrackingRecord.ToJson(new Sycamore_TrackingRecord());

        TrackingController trackingController = new TrackingController();

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{0}", "error");
            Response.Write(response.ToString());
            return;
        }

        // Data input Check
        if (package == "" || package == null)
        {
            response.AppendFormat("{0}", "error");
            Response.Write(response.ToString());
            return;
        }

        try
        {
            Random rd = new Random();
            double lat = -37.744313 + rd.NextDouble() / 1000;
            double lon = 144.967388 + rd.NextDouble() / 1000;
            string gps = "1,1,20210506021643.000," + lat.ToString() + "," + lon.ToString() + ",376.900,0.00,73.1,1,,500.0,500.0,500.0,,22,2,,,,35,,";
            string package_random = package + "_A_35_2000_D_E_F_G_H_I_J_" + gps;
            string link = "https://fleetdev.allinks.com.au/api/OnSendVehicleTracking.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&package=" + package_random;
            WebRequest request = WebRequest.Create(link);
            request.GetResponse();
            response.AppendFormat("\"message\":\"{0}\"|||||", link);

        }
        catch (Exception ex)
        {
            response.AppendFormat("{0}", "error: " + ex.StackTrace);
            // System Log
            SystemLog log = new SystemLog(key, "OnSendVehicleTracking.aspx", "Record Tracking Data", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}