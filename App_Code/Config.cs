#define DEV // Remove this for production server

using Amazon.DynamoDBv2.Model;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Config
/// </summary>
public class Config
{
    public static string ProductName = "Allink Fleet Management";
#if (!DEV)
    public static string Site = "";
    public static string AWS_IdentityPoolId = "ap-southeast-2:62bd97fc-2d3c-4491-8b60-75710db586af";
    public static string DynamoDB_Endpoint = "http://localhost:7000";
    public static string DynamoDB_Auth = "master";
    public static string DynamoDB_Regoin = "ap-southeast-2";
    public static string DevelopmentStage = "production";   // change to production when deploy
    public static string DevelopmentGroup = "production";   // change to admin when deploy
#else
    // Service production

    public static string Site = "Dev_";// currently the dev site is the production site for this project
    public static string AWS_IdentityPoolId = "ap-southeast-2:04e37a7a-19cc-46f5-a632-aea81e0430f8";  // IdentityPool Id From darkspede.dev@gmail.com
    public static string DynamoDB_Endpoint = "http://localhost:7000";
    public static string DynamoDB_Auth = "master";
    public static string DynamoDB_Regoin = "ap-southeast-2";
    public static string DevelopmentStage = "dev";  
    public static string DevelopmentGroup = "dev";
    public static bool IsDebugEnabled = false;
#endif


    // Table list of current design, add new table in this list only
    public static Dictionary<string, string> TableList = new Dictionary<string, string>()
    {
        { "Sycamore_User","Sycamore_FleetManagement" },
        { "Sycamore_Insight","Sycamore_FleetManagement" },
        { "Sycamore_Report","Sycamore_FleetManagement" },
        { "Sycamore_Notification","Sycamore_FleetManagement" },
        { "Sycamore_Operation", "Sycamore_FleetManagement" },
        { "Sycamore_Operation_Request", "Sycamore_FleetManagement" },
        { "Sycamore_Driver", "Sycamore_FleetManagement" },
        { "Sycamore_DriverRate", "Sycamore_FleetManagement" },
        { "Sycamore_Group", "Sycamore_FleetManagement" },
        { "Sycamore_Fleet", "Sycamore_FleetManagement" },
        { "Sycamore_Vehicle", "Sycamore_FleetManagement" },
        { "Sycamore_Service", "Sycamore_FleetManagement" },
        { "Sycamore_Document", "Sycamore_FleetManagement" },
        { "Sycamore_ServiceProvider", "Sycamore_FleetManagement" },
        { "Sycamore_TrackingRecord", "Sycamore_FleetManagement" },
        { "Sycamore_TrackingPackage", "Sycamore_FleetManagement" },
        { "Sycamore_OperationQR", "Sycamore_FleetManagement" },
    };

    public static int INPUT_LENGTH = 12; // Send package length
    public static string AUTH_CODE_SURFIX = " is your Allink security code. it expires in 10 minutes. DO NOT share this code with anyone.";
    public static string REQUEST_PREFIX = "Allink Fleet User [";
    public static string REQUEST_SURFIX = "] is agreed to contact with you via mobile number -  ";
    public static string securityCode = "********";
    public static int TokenExpire = 5; // days, added when renew token
    public static int CodeExpire = 10; // minutes, added when renew code
    public static string Resource_PostImage = "/resource/post/";
    public static string Resource_UserImage = "/resource/user/";
    public static string Resource_MiscImage = "/resource/misc/";
    public static string Resource_SystemLog = "/resource/systemLog/";
    public static string smsLink = "https://fleetdev.allinks.com.au/api/OnRequestSendSMS.aspx?key=272T0ymkF0KixH3QmaXMwQ&target=";

    public static string API_MapboxGeoCoding = "https://api.mapbox.com/geocoding/v5/mapbox.places/";
    public static string API_MapboxAccessToken = "pk.eyJ1IjoieXVoZTA5MjUiLCJhIjoiY2tkbnJobWM2MHY2ZTJycWltNG1lbm5xNyJ9.OPHhWDcFai-mdAqNLzhATA";
    public static string API_GoogleGeoCoding = "https://maps.googleapis.com/maps/api/place/textsearch/json?query=";
    public static string API_GoogleAPIKEY = "AIzaSyAqtQk-7uH6izNU-mC3zby9nGvrHxMDjuI"; // using sycamore.master@gmail.com

    public static string API_FCM_API = "https://fcm.googleapis.com/fcm/send";
    public static string API_FCM_AuthorizationKey = "AAAAdVWtPJs:APA91bG94plrR3q76vnz-fDhnQh35bbVPUgSTn0ckrDWQSwQUBZLFNKV17QECbGTXuLOfTQVzh7OBcTatyS2q_kOy4mfnfdtXbPuqztLTBmhogt17YZURJQVXsroYzOXVMrRzAzPDuCz";
    public static string API_FCM_SenderID = "503948590235";

    public static int ResizeImageMaxWidth = 1024;
    public static int ResizeImageMaxHeight = 1024;

    public static string LogFileNameDateFormat = "yyyyMMdd";
    public static string LogDateTimeFormat = "yyyy-MM-dd HH-mm-ss";

    public static int RecordQueueMaxCount = 100;

    public Config()
    {
        //
        // TODO: Add constructor logic here
        //
    }




    // HardCoded Key, only for dev

    private static List<string> SecureKeyList = new List<string>()
    {
        "j1RpMum08UKS1oT7r7E1bA",
        "272T0ymkF0KixH3QmaXMwQ",
        "xVgsbCuuxkCGFDwLk1EpEQ"
    };

    private static Dictionary<string, string> ExceptionUserGroup = new Dictionary<string, string>()
    {
        { "61400000001","000000"}
    };


    public static bool CheckKey(string _key)
    {
        return SecureKeyList.Contains(_key);
    }


    public static bool CheckUserException(string _key)
    {
        return ExceptionUserGroup.ContainsKey(_key);
    }





    // API Document Region
    #region

    public static string EmptyObject = "Empty"; // Empty object in API Document
    public static string APIDocumentName = "APIDocument.md";

    // HardCoded Key, only for dev
    public static Dictionary<string, string> API_Documentation_Prefix = new Dictionary<string, string>()
    {
        { "key","Service key provided by publisher or the user guid"},
        { "guid","Index key as UUID of an item"},
        { "filterName","Name of the filter used for table scanning"},
        { "filterValue","Value of the filter used for table scanning"},
        { "package","Package data formated in Json String of an item(object)"},
        { "mobile", "User mobile number"},
        { "tableName", "Name of table" },
        //Default, repeat the name of request
    };

    public static String API_Documentation_OutpoutPrefix = String.Format("{{\"success\":\"[true | false]\", \"message\":\"{0}\", \"package\":{1}}}",
                "[Message returned by API]", "[Data payload returned by API in Json Format]");


    public static string apiUrl = "http://fleetdev.allinks.com.au/api/";
    public static string Resource_APIDocument = "/resource/apiDocument/";

    #endregion
}