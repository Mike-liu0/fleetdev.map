using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using Darkspede;

//https://fleetdev.allinks.com.au/api/OnSendVehicleTracking.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&package=
//[packageID]_[deviceID]__[GPS STRING]
//1_D000-0001_A_35_2000_D_E_F_G_H_I_J_K_GPS
public partial class OnSendVehicleTracking : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string package = Request["package"];


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
            string temp = package;
            string[] dataArray = temp.Split('_');

            // 1 GSP data, 7 obd data, 1 status data
            if (dataArray.Length > Config.INPUT_LENGTH)//MARKER : HARD CODE
            {
                Sycamore_TrackingRecord Tracking = new Sycamore_TrackingRecord();
                Tracking.attachedDevice = dataArray[1];
                Tracking.rawData = package;

                trackingController.ProcessRAW(Tracking);
                if (TrackingController.Instance != null)
                {
                    TrackingController.Instance.AddRecords(dataArray[1], Tracking);
                }else
                {
                    TrackingController.Instance = new TrackingController();
                }
            }

            
            // Request for operation cmd

            response.AppendFormat("{0}", "done");


            SystemLog log = new SystemLog(key, "OnSendVehicleTracking.aspx", "Record Tracking Data", "true", package);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
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