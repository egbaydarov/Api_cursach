using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models
{
    public class Note
    {
        public Note(string description, string imageReference)
        {
            Description = description;
            ImageReference = imageReference;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "imagereference")]
        public string ImageReference { get; set; }
    }
}
