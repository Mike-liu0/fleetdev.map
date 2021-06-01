using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Text;
using Darkspede;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;

public partial class OnAPIDocumentationUpdate : System.Web.UI.Page
{
    public static string OUTPUT = Config.EmptyObject;
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder response = new StringBuilder();
        APIController apiController = new APIController();


        string emptyObject = "{}";  
        string fileName = "OnAPIDocumentationUpdate.aspx.cs";
        string fileDirectory;
        string parentDirectory;

        // Get file directory
        fileDirectory = Server.MapPath(fileName);
        // Get file parent directory
        parentDirectory = apiController.GetFileParentDirectory(fileDirectory);
        // Get all "*.aspx.cs" files info
        FileInfo[] Files = apiController.GetFileInfo(parentDirectory);


        // If no api file found, return false
        if (Files.Length <= 0)
        {
            response.AppendFormat("{{\"success\":\"false\", \"message\":\"{0}\", \"package\":{1}}}",
                "No API file found", JObject.Parse(emptyObject));    //Write response into string
            Response.Write(response.ToString());
            return;
        }

        response.AppendFormat("# API Document" + "  \n");

        string apiInputPattern = @"Request\[(.*?)\]";
        string apiObjectPattern = @"// API Object \[(.*?)\]";
        string line;

        Regex apiInputRegex = new Regex(apiInputPattern);
        Regex apiObjectRegex = new Regex(apiObjectPattern);


        foreach (FileInfo file in Files)
        {
            string dir = file.FullName.ToString();
            string matchedObject = null;
            string outputObject = apiController.GetAPIOutput(file);
            response.AppendFormat("## {0}" + "  \n", file.Name.Replace(".cs", ""));

            using (StreamReader reader = new StreamReader(dir))
            {
                List<string> matchedList = new List<string>();
                response.AppendFormat("### Input Parameters" + "  \n");
                while ((line = reader.ReadLine()) != null)
                {
                    MatchCollection apiInputMatches = apiInputRegex.Matches(line); // API Input Match
                    MatchCollection apiObjectMatches = apiObjectRegex.Matches(line); // API Object Match
          
                    foreach (Match match in apiInputMatches)
                    {
                        GroupCollection groups = match.Groups;
                        string matchedWord = groups[1].ToString().Trim('"');
                        if (matchedWord != "output")
                        {
                            response.AppendFormat(" - **{0}:** {1}" + "  \n", matchedWord, apiController.GetInputWithExplation(matchedWord));
                            matchedList.Add(matchedWord);
                        }
                    }

                    foreach (Match match in apiObjectMatches)
                    {
                        GroupCollection groups = match.Groups;
                        matchedObject = groups[1].ToString();
           
                    }

                }
                response.AppendFormat("### Usage" + "  \n");
                response.AppendFormat("http://{0}?{1}" + "  \n", apiController.GetModifiedDirectory(dir), apiController.GetInputListWithValue(matchedList));
            }
            response.AppendFormat("### API Object" + "  \n");
            response.AppendFormat("`" + "{0}" + "`" + "  \n", matchedObject);
            response.AppendFormat("### Output" + "  \n");
            response.AppendFormat("```json" + "  \n");
            response.AppendFormat("{0}" + "  \n", apiController.GetAPIOutputWithObject(outputObject));
            response.AppendFormat("```" + "  \n");
            response.AppendFormat("  \n");
        }

        //string apiDocmentName = "APIDocument.md";
        //var stream = new MemoryStream();
        string filePath = Server.MapPath("~") + Config.Resource_APIDocument + Config.APIDocumentName;
        File.WriteAllText(filePath, response.ToString());
        Response.Write("http://fleetdev.allinks.com.au/resource/apiDocument/APIDocument.md");
        return;
    }
}
