using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models
{
    public class Note
    {
        public Note(string description, string imageReference, List<string> eximages = null)
        {
            Description = description;
            ImageReference = imageReference;
            LukasCount = 0;
            Lukasers = new List<string>();
            ExImages = eximages;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "imagereference")]
        public string ImageReference { get; set; }

        [JsonProperty(PropertyName = "lukascount")]
        public int LukasCount { get; set; }

        [JsonProperty(PropertyName = "lukasers")]
        public  List<string>  Lukasers { get; set; }

        [JsonProperty(PropertyName = "eximages")]
        public List<string> ExImages { get; set; }
    }
}
