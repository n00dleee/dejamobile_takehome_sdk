using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Models
{
    public class AuthJwtModel
    {
        [JsonProperty("success")]
        public string success { get; set; }

        [JsonProperty("err")]
        public string err { get; set; }

        [JsonProperty("token")]
        public string token { get; set; }
    }
}
