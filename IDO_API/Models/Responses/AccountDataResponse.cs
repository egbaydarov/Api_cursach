using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models.Responses
{
    public class AccountDataResponse : Response
    {
        public AccountDataResponse(int status, User data) : base(status)
        {
            Data = data;
        }

        [JsonProperty(PropertyName = "data")]
        public User Data { get; set; }
    }
}
