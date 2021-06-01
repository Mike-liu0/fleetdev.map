using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Class1
/// </summary>
public class FCM_SMS
{
    public string FCM_Code { get; set; }
    public string FCM_Message { get; set; }
    public string FCM_Action { get; set; }
    public string FCM_To { get; set; }
    public string FCM_From { get; set; }

    public FCM_SMS(string _message = "Testing SMS from Sycamore Web Platform, DO NOT replay", string _target = "+61433280815")
    {
        FCM_Message = _message;
        FCM_To = _target;

        FCM_Code = "80808";
        FCM_Action = "SMS";
        FCM_From = "Sycamore Connected";
    }

    public static string ToJson(FCM_SMS _item)
    {
        return JsonConvert.SerializeObject(_item);
    }

    public static FCM_SMS FromJson(string _json)
    {
        return JsonConvert.DeserializeObject<FCM_SMS>(_json);
    }


}