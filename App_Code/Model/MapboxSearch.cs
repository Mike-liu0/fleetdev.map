using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Darkspede.Sycamore.GeoSearch
{
    [Serializable]
    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    [Serializable]
    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    [Serializable]
    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    [Serializable]
    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }
    [Serializable]
    public class Geometry
    {
        public Location location { get; set; }
        public Viewport viewport { get; set; }
    }
    [Serializable]
    public class Photo
    {
        public int height { get; set; }
        public IList<string> html_attributions { get; set; }
        public string photo_reference { get; set; }
        public int width { get; set; }
    }
    [Serializable]
    public class PlusCode
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }
    [Serializable]
    public class OpeningHours
    {
        public bool open_now { get; set; }
    }
    [Serializable]
    public class Result
    {
        public string business_status { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public bool permanently_closed { get; set; }
        public IList<Photo> photos { get; set; }
        public string place_id { get; set; }
        public PlusCode plus_code { get; set; }
        public double rating { get; set; }
        public string reference { get; set; }
        public IList<string> types { get; set; }
        public int user_ratings_total { get; set; }
        public OpeningHours opening_hours { get; set; }
    }
    [Serializable]
    public class Root
    {
        public IList<object> html_attributions { get; set; }
        public string next_page_token { get; set; }
        public IList<Result> results { get; set; }
        public string status { get; set; }
    }
}
