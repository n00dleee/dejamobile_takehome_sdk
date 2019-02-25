using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Models
{
    public class UserModel
    {
        [JsonProperty("userName")]
        public string userName { get; set; }

        [JsonProperty("firstName")]
        public string firstName { get; set; }

        [JsonProperty("lastName")]
        public string lastName { get; set; }

        [JsonProperty("phoneNumber")]
        public string phoneNumber { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        public UserModel(string userName, string password, string firstName="", string lastName="", string phoneNumber="")
        {
            this.userName = userName;
            this.password = password;
            this.firstName = firstName;
            this.lastName = lastName;
            this.phoneNumber = phoneNumber;
        }
    }
}
