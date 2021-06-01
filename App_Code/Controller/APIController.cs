using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Summary description for APIController
/// </summary>
public class APIController
{
    public APIController()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    // Get file parent directory 
    public string GetFileParentDirectory(string fileDirectory)
    {
        return Directory.GetParent(fileDirectory).FullName;
    }

    // Get all aspx.cs files info, including its subdirectories
    public FileInfo[] GetFileInfo(string parentDirectory)
    {
        DirectoryInfo d = new DirectoryInfo(parentDirectory);
        FileInfo[] Files = d.GetFiles("*.aspx.cs", SearchOption.AllDirectories);
        return Files;
    }

    // Return API Link
    public string GetModifiedDirectory(string dir)
    {
        string result;
        result = dir.Replace(@"C:\Dropbox\SycamoreConnect\", "").Replace(@"\", "/").Replace(".cs", "");

        return result;
    }

    // Input
    public string GetInputListWithValue(List<string> list)
    {
        switch (list.Count)
        {
            case 0:
                return null;
            case 1:
                return list[0] + "=xVgsbCuuxkCGFDwLk1EpEQ";
            case 2:
                return list[0] + "=xVgsbCuuxkCGFDwLk1EpEQ&" + list[1] + "=";
            case 3:
                return list[0] + "=xVgsbCuuxkCGFDwLk1EpEQ&" + list[1] + "=" + "&" + list[2] + "=";
            case 4:
                return list[0] + "=xVgsbCuuxkCGFDwLk1EpEQ&" + list[1] + "=" + "&" + list[2] + "=" + "&" + list[3] + "=";
        }
        return null;
    }

    public string GetInputWithExplation(string input)
    {
        var inputList = Config.API_Documentation_Prefix;

        foreach (var match in inputList)
        {
            if (input == match.Key)
            {
                return match.Value;
            }
        }
        return null;
    }

    public string GetAPIOutputWithObject(string _object)
    {
        if (_object != null)
        {
            return Config.API_Documentation_OutpoutPrefix.Replace("[Data payload returned by API in Json Format]", _object);
        }
        else
        {
            return Config.API_Documentation_OutpoutPrefix;
        }
    }

    public string GetAPIOutput(FileInfo file)
    {
        string securityKey = "key=xVgsbCuuxkCGFDwLk1EpEQ";
        string outputName = "&output=output";
        string outputObject;

        try
        {
            var webClient = new WebClient
            {
                Proxy = null
            };
            outputObject = webClient.DownloadString(Config.apiUrl + file.Name.Replace(".cs", "?") + securityKey + outputName);
        }
        catch (Exception)
        {
            outputObject = null;
        }
        return outputObject;
    }

}