using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede;


// 

public partial class AddNewGroup : System.Web.UI.Page
{
    public string OUTPUT = Sycamore_Group.ToJson(new Sycamore_Group());
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        string key = Request["key"];
        string output = Request["output"];
        string package = Request["package"];

        Sycamore_Group package_item = new Sycamore_Group();

        //Security check
        if (!Config.CheckKey(key))    // Check failed
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Authentication fail", Sycamore_Group.ToJson(package_item));    //Write response into string
            Response.Write(response.ToString());
            return;
        }

        if (output == "output")
        {
            response.AppendFormat("{0}", Sycamore_Group.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }

        //Security check succeed
        //Parse data input
        if (package == "" || package == null)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "Data input fail", Sycamore_Group.ToJson(package_item));
            Response.Write(response.ToString());
            return;
        }


        try
        {
            // remove excape from json encoder (unity JsonUtility)
            package.Replace("\\", "");
            Sycamore_Group item = Sycamore_Group.FromJson(package);

            
            string result = new AWSController().OnDynamoDB_AddNewGroup(item);
            Guid guidResult;
            bool isValid = Guid.TryParse(result, out guidResult);
            
            if (isValid)
            {
                response.AppendFormat("{{\"success\":\"true\", \"message\":\"{0}\", \"package\":{1}}}",
                    "API.AddNewGroup Complete", Sycamore_Group.ToJson(item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS add item", "true");
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            else
            {
                response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                    "Database Error: " + result, Sycamore_Group.ToJson(package_item));

                // System Log
                SystemLog log = new SystemLog(key, this.GetType().Name, "AWS add item", "false", result);
                LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
            }
            
        }
        catch (Exception ex)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "API Error: " + ex.Message, Sycamore_Group.ToJson(package_item));

            // System Log
            SystemLog log = new SystemLog(key, this.GetType().Name, "AWS add item", "false", ex.Message);
            LogController.WriteToLog(Server.MapPath(Config.Resource_SystemLog), log);
        }

        Response.Write(response.ToString());
        return;
    }
}
