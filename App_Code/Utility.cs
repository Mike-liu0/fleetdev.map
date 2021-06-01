using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/// <summary>
/// Summary description for Utility
/// </summary>
public class Utility
{
    public static Regex EMAIL_PATTERN = new Regex(@"/^ ([\w -] + (?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w -]{0,66})\.([a - z]{2,6}(?:\.[a-z]{2})?)$/i");
    public static Regex PHONE_PATTERN_AUS_1 = new Regex(@"/\+614[0-9]{8}$/");
    public static Regex PHONE_PATTERN_AUS_2 = new Regex(@"/04[0 - 9]{8}$/");
    public static Regex PHONE_PATTERN_AUS_3 = new Regex(@"/4[0 - 9]{8}$/");
    public static Regex PHONE_PATTERN_AUS_4 = new Regex(@"/614[0-9]{8}$/");
    public static string COUNTRY_AUS = "aus";
    public static Encoding GB2312 = Encoding.GetEncoding("GB2312");



    /// <summary>
    /// Checking the format and validating the mobile input
    /// </summary>
    /// <param name="_mobile"></param>
    /// <param name="_country">The short extension of country, example "aus" = Australia</param>
    /// <returns>Return "" if the mobile is not valid, otherwise return the normalized number</returns>
    public static string ValidateMobile(string _mobile, string _country = "aus")
    {
        string number = _mobile;

        if (_country == COUNTRY_AUS)
        {
            _mobile = _mobile.Replace(" ", "");
            _mobile = _mobile.Replace("-", "");
            _mobile = _mobile.Replace("+", "");


            if (_mobile.StartsWith("614") && _mobile.Length == 11)
            {
                if (_mobile.All(char.IsDigit))
                {
                    return _mobile;
                }
                return "";
            }

            if (_mobile.StartsWith("04") && _mobile.Length == 10)
            {
                _mobile = _mobile.Remove(0, 1);
                _mobile = "61" + _mobile;
                if (_mobile.All(char.IsDigit))
                {
                    return _mobile;
                }
                return "";
            }

            if (_mobile.StartsWith("4") && _mobile.Length == 9)
            {
                _mobile = "61" + _mobile;
                if (_mobile.All(char.IsDigit))
                {
                    return _mobile;
                }
                return "";
            }


        }

        //other countries
        return null;
    }

    /* ============================================================================
     * =================== Date Time Process CH/ZN ================================
     * ============================================================================
     */
    #region  Date Time Process CH/ZN

    public static List<string> CurrentDate()
    {
        string year = System.DateTime.Now.Year.ToString();
        string month = System.DateTime.Now.Month.ToString();
        string day = System.DateTime.Now.Day.ToString();
        string dayofWeek = System.DateTime.Now.DayOfWeek.ToString();
        string temp = "null";
        var currentDate = new List<string>();
        currentDate.Add(month);
        currentDate.Add(day);
        currentDate.Add(year);


        switch (dayofWeek)
        {
            case "Monday":
                temp = "一";
                break;
            case "Tuesday":
                temp = "二";
                break;
            case "Wednesday":
                temp = "三";
                break;
            case "Thursday":
                temp = "四";
                break;
            case "Friday":
                temp = "五";
                break;
            case "Saturday":
                temp = "六";
                break;
            case "Sunday":
                temp = "日";
                break;
            default:
                break;

        }
        currentDate.Add(temp);

        return currentDate;

    }
    public static DateTime DataStringToData(string _dateString)
    {
        DateTime result = Convert.ToDateTime(_dateString);
        return result;
    }

    /// <summary>
    /// get the day of the week in ZH chinese
    /// 获取中文星期几
    /// </summary>
    /// <param name="_date"></param>
    /// <returns></returns>
    public static string GetDayofWeekZH(DateTime _date)
    {
        string temp = "null";

        switch (_date.DayOfWeek.ToString())
        {
            case "Monday":
                temp = "星期一";
                break;
            case "Tuesday":
                temp = "星期二";
                break;
            case "Wednesday":
                temp = "星期三";
                break;
            case "Thursday":
                temp = "星期四";
                break;
            case "Friday":
                temp = "星期五";
                break;
            case "Saturday":
                temp = "星期六";
                break;
            case "Sunday":
                temp = "星期天";
                break;
            default:
                break;

        }

        return temp;
    }

    public static string GetDayDiffFromNow(string _updated)
    {
        DateTime.Parse(_updated);
        int day = (int)(DateTime.Now - DateTime.Parse(_updated)).TotalDays;
        if (day == 0)
        {
            return "[今天]";
        }
        else
        {
            return "[" + day + "天前]";
        }


    }

    public static int GetHourDiffFromNow(DateTime _to)
    {
        return (DateTime.Now - _to).Hours;
    }

    public static int GetMinuteDiffFromNow(DateTime _to)
    {
        return (DateTime.Now - _to).Minutes;
    }

    public static int GetTimeDiffInMinutes(DateTime _Start, DateTime _End)
    {
        DateTime startTime = _Start;
        DateTime endTime = _End;
        TimeSpan span = endTime.Subtract(startTime);
        return (int)span.TotalMinutes;
    }

    public static double DateTimeDiffMinutes(DateTime _a, DateTime _b)
    {
        return (_b - _a).TotalMinutes;
    }

    public static double DateTimeDiffSeconds(DateTime _a, DateTime _b)
    {
        return (_b - _a).TotalSeconds;
    }

    #endregion



    /* ============================================================================
     * =================== not yet put in category ================================
     * ============================================================================
     */

    public static bool SecurityCheck()
    {
        return false;
    }

    public static String GetSHA(String _value)
    {
        StringBuilder Sb = new StringBuilder();

        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(enc.GetBytes(_value));

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
        }

        return Sb.ToString();
    }


    /// <summary>
    /// Get config value from AppSetting in Web.config
    /// </summary>
    /// <param name="_configKey"></param>
    /// <returns></returns>
    public static string Config(string _configKey)
    {
        return (string)ConfigurationManager.AppSettings[_configKey];
    }


    public static string GetTempCelsius(float _kelvin)
    {
        float result = 0;
        result = _kelvin - 273.15f;
        return result.ToString("0.0");
    }

    public static string GetNewGUID()
    {
        return Guid.NewGuid().ToString();
    }


    public static string GetExcelColumnName(int columnNumber)
    {
        int dividend = columnNumber;
        string columnName = String.Empty;
        int modulo;

        while (dividend > 0)
        {
            modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
            dividend = (int)((dividend - modulo) / 26);
        }

        return columnName;
    }

    public static int GetExcelColumnNumber(string columnName)
    {

        if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName");
        columnName = columnName.ToUpperInvariant();
        int sum = 0;

        for (int i = 0; i < columnName.Length; i++)
        {
            sum *= 26;
            sum += (columnName[i] - 'A' + 1);
        }

        return sum;
    }




    public static List<string> GetStringListFromString(string _string)
    {
        List<string> StringList = new List<string>();
        string newString = _string.Remove(0, 1);
        newString = newString.Remove(newString.Length - 1, 1);
        string[] list = newString.Split(',');
        foreach (string line in list)
        {
            StringList.Add(line);
        }
        return StringList;
    }

    public static string GetStringFromStringList(List<string> _list)
    {
        string s = "[";

        int i = 0;
        foreach (string line in _list)
        {
            if (i == 0)
            {
                s = s + line;
            }
            else
            {
                s = s + "," + line;
            }
            i++;
        }

        s = s + "]";
        return s;
    }


    /// <summary>
    /// Using tick
    /// </summary>
    /// <returns></returns>
    public static float GetDaysBetweenTwoDate(long _created, long _updated)
    {
        return (_updated - _created) / 10000000 / 86400;
    }



    public static string GenerateSecurityCode(int _number = 6)
    {
        string code = "";
        Random rnd = new Random();

        for (int index= 0; index < _number; index++)
        {
            code+= rnd.Next(10);
        }

        return code;
    }


    // <summary>
    /// Resize the image to the specified width and height.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
    }

    public static byte[] ImageToByte(Image img)
    {
        ImageConverter converter = new ImageConverter();
        return (byte[])converter.ConvertTo(img, typeof(byte[]));
    }


    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

}