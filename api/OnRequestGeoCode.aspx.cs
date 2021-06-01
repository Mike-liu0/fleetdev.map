using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede.Sycamore;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Darkspede;


//https://fleetdev.allinks.com.au/api/OnRequestGeoCode.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&longitude=144.95673789084&latitude=-37.8108019733089
public partial class OnRequestGeoCode : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string longitude = Request["longitude"];
        string latitude = Request["latitude"];

        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Authentication failed", FCM_SMS.ToJson(new FCM_SMS()));
            Response.Write(response.ToString());
            return;
        }

        try
        {
            string link = Config.API_MapboxGeoCoding + longitude + "," + latitude + ".json?access_token=" + Config.API_MapboxAccessToken;
            string result;
            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(link);
            // If required by the server, set the credentials.  
            request.Credentials = CredentialCache.DefaultCredentials;

            // Get the response.  
            WebResponse RequestResponse = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)RequestResponse).StatusDescription);

            // Get the stream containing content returned by the server. 
            // The using block ensures the stream is automatically closed. 
            using (Stream dataStream = RequestResponse.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                result = responseFromServer;
            }

            Darkspede.Sycamore.GeoCoding.Root myDeserializedClass = JsonConvert.DeserializeObject<Darkspede.Sycamore.GeoCoding.Root>(result);


            response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":\"{1}\"}}", "API.OnRequestGeoCode Complete", myDeserializedClass.features[0].place_name);

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestGeoCode.aspx", "request geo code", "true");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":\"{0}\"}}", "API Error: " + ex.Message);

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestGeoCode.aspx", "request geo code", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }


        
        Response.Write(response.ToString());
        return;
    }
}