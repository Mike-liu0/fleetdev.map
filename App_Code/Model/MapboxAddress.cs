using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darkspede.Sycamore.GeoCoding
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    [Serializable]
    public class Properties
    {
        public string accuracy { get; set; }
        public string wikidata { get; set; }
        public string short_code { get; set; }
    }
    [Serializable]
    public class Geometry
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }
    [Serializable]
    public class Context
    {
        public string id { get; set; }
        public string text { get; set; }
        public string wikidata { get; set; }
        public string short_code { get; set; }
    }

    [Serializable]
    public class Feature
    {
        public string id { get; set; }
        public string type { get; set; }
        public List<string> place_type { get; set; }
        public int relevance { get; set; }
        public Properties properties { get; set; }
        public string text { get; set; }
        public string place_name { get; set; }
        public List<double> center { get; set; }
        public Geometry geometry { get; set; }
        public string address { get; set; }
        public List<Context> context { get; set; }
        public List<double> bbox { get; set; }
    }

    [Serializable]
    public class Root
    {
        public string type { get; set; }
        public List<double> query { get; set; }
        public List<Feature> features { get; set; }
        public string attribution { get; set; }
    }


}
