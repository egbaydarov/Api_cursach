using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    
    public class SimpleResponse : Response
    {
        public SimpleResponse(int status = 0, string message = "OK"): base (status)
        {
            Message = message;
        }


        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

    }
}
