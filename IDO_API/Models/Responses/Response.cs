using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    public class Response
    {
        public Response(int status)
        {
            Status = status;
        }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
