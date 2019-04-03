using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models
{
    public class Content
    {
        public Content(string id, List<Note> notes)
        {
            Id = id;
            this.notes = notes;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }


        [JsonProperty(PropertyName = "notes")]
        public List<Note> notes { get; set; }

    }
}
