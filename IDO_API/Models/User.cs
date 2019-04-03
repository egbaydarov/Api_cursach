using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.Models
{
    public class User
    {
        public User(string nickname, string password)
        {
            Nickname = nickname;
            Password = password;
            Follows = new List<string>();
            Followers = new List<string>();

        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "follows")]
        public List<string> Follows { get; set; }

        [JsonProperty(PropertyName = "followers")]
        public List<string> Followers { get; set; }
    }
}
