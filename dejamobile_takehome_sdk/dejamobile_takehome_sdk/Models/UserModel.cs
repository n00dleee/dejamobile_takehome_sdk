using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Models
{
    public class UserModel
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public UserModel(string userName, string password)
        {
            this.Username = userName;
            this.Password = password;
        }
    }
}
