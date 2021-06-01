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



public partial class OnRequestGeoSearch : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string longitude = Request["longitude"];
        string latitude = Request["latitude"];
        string searchString = Request["searchString"];


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

        //link = Config.API_MapboxGeoCoding + searchString + ".json?" + "types=address&" + "proximity=" + longitude + "," + latitude + "&access_token=" + Config.API_MapboxAccessToken;
        //https://maps.googleapis.com/maps/api/place/textsearch/json?query=mark&key=AIzaSyAqtQk-7uH6izNU-mC3zby9nGvrHxMDjuI
        link = Config.API_GoogleGeoCoding + searchString + "&key=" + Config.API_GoogleAPIKEY;


        try
        {
            //string link = Config.API_MapboxGeoCoding + longitude + "," + latitude + ".json?access_token=" + Config.API_MapboxAccessToken;


            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(@link);
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

            Sycamore_AddressList list = new Sycamore_AddressList();
            list.package = new List<Sycamore_Address>();

            Darkspede.Sycamore.GeoSearch.Root myDeserializedClass = JsonConvert.DeserializeObject<Darkspede.Sycamore.GeoSearch.Root>(result);

     

            resultString = "[";

            int index = 0;
            foreach(Darkspede.Sycamore.GeoSearch.Result feature in myDeserializedClass.results)
            {
                Sycamore_Address address = new Sycamore_Address();
                address.siteAddress = feature.formatted_address;
                address.siteCoordinatelatitude = feature.geometry.location.lat;
                address.siteCoordinatelongitude = feature.geometry.location.lng;
                list.package.Add(address);

                /*
                if (index != 0)
                {
                    resultString = resultString + "," + feature.formatted_address.Replace(",", " ");
                }
                else
                {
                    resultString = resultString + feature.formatted_address.Replace(",", " ");
                }
                  */
                index++;

            }

            resultString = resultString + "]";

            response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}", "API.OnRequestGeoCode Complete", Sycamore_AddressList.ToJson(list));

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestGeoSearch.aspx", "request geo search", "true");
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":\"{1}\"}}", ex.StackTrace, link);

            // System Log
            SystemLog log = new SystemLog(key, "OnRequestGeoSearch.aspx", "request geo search", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }


        
        Response.Write(response.ToString());
        return;
    }
}