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


//https://fleetdev.allinks.com.au/api/OnRequestGeoLocation.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&address="17 Singers Lane, Melbourne Victoria 3000, Australia"
public partial class OnRequestGeoLocation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string address = Request["address"];

        string resultString = "";
        string link = "";
        string result = "";
        // Security Check
        if (!Config.CheckKey(key))
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}", "Authentication failed", FCM_SMS.ToJson(new FCM_SMS()));
            Response.Write(response.ToString());
            return;
        }

        try
        {
            link = Config.API_MapboxGeoCoding + address + ".json?" + "types=address&" + "&access_token=" + Config.API_MapboxAccessToken;
            //string link = Config.API_MapboxGeoCoding + longitude + "," + latitude + ".json?access_token=" + Config.API_MapboxAccessToken;

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
            SystemLog log = new SystemLog(key, "OnRequestGeoLocation.aspx", "request geo location", "true");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":\"{1}\"}}", "API Error: " + ex.Message, link);

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestGeoLocation.aspx", "request geo location", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }


        
        Response.Write(response.ToString());
        return;
    }
}