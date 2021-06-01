using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Darkspede.Sycamore;
using Newtonsoft.Json;

/// <summary>
/// Summary description for FCMController
/// </summary>
public class FCMController
{
    public FCMController()
    {

    }


    public string OnSendFCMNotifucation(string _token, string _message, string _targetGuid, string _userGuid, string _title = "Allink")
    {
        WebRequest tRequest = WebRequest.Create(Config.API_FCM_API);
        tRequest.Method = "post";
        //serverKey - Key from Firebase cloud messaging server  
        tRequest.Headers.Add(string.Format("Authorization: key={0}", Config.API_FCM_AuthorizationKey));
        //Sender Id - From firebase project setting  
        tRequest.Headers.Add(string.Format("Sender: id={0}", Config.API_FCM_SenderID));
        tRequest.ContentType = "application/json";
        var payload = new
        {
            to = _token,
            priority = "high",
            content_available = true,
            notification = new
            {
                body = _message,
                title = _title,
                badge = 0
            },
            data = new
            {
                postGuid = _targetGuid,
                userGuid = _userGuid
            }

        };

        string postbody = JsonConvert.SerializeObject(payload).ToString();
        Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
        tRequest.ContentLength = byteArray.Length;

        using (Stream dataStream = tRequest.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
            using (WebResponse tResponse = tRequest.GetResponse())
            {
                using (Stream dataStreamResponse = tResponse.GetResponseStream())
                {
                    if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                        {
                            String sResponseFromServer = tReader.ReadToEnd();
                            //result.Response = sResponseFromServer;
                        }
                }
            }
        }

        return postbody;
    }


    public string OnSendFCMNotifucation_System(string _message, string _title = "Allink")
    {
        WebRequest tRequest = WebRequest.Create(Config.API_FCM_API);
        tRequest.Method = "post";
        //serverKey - Key from Firebase cloud messaging server  
        tRequest.Headers.Add(string.Format("Authorization: key={0}", Config.API_FCM_AuthorizationKey));
        //Sender Id - From firebase project setting  
        tRequest.Headers.Add(string.Format("Sender: id={0}", Config.API_FCM_SenderID));
        tRequest.ContentType = "application/json";
        var payload = new
        {
            //to = _token,
            priority = "high",
            content_available = true,
            notification = new
            {
                body = _message,
                title = _title,
                badge = 0
            },
            data = new
            {
                //postGuid = _targetGuid,
                userGuid = "000"
            },
            condition = "!('anytopicyoudontwanttouse' in topics)"

        };

        string postbody = JsonConvert.SerializeObject(payload).ToString();
        Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
        tRequest.ContentLength = byteArray.Length;

        using (Stream dataStream = tRequest.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
            using (WebResponse tResponse = tRequest.GetResponse())
            {
                using (Stream dataStreamResponse = tResponse.GetResponseStream())
                {
                    if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                        {
                            String sResponseFromServer = tReader.ReadToEnd();
                            //result.Response = sResponseFromServer;
                        }
                }
            }
        }

        return postbody;
    }
}