using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace dejamobile_takehome_sdk.Models
{
    public class CardModel
    {
        [JsonProperty("ownerName")]
        public string ownerName { get; set; }

        [JsonProperty("cardNumber")]
        public string cardNumber { get; set; }

        [JsonProperty("expirationDate")]
        public string expirationDate { get; set; }

        [JsonProperty("crypto")]
        public string crypto { get; set; }

        [JsonProperty("productionDate")]
        public string productionDate { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("uid")]
        public string uid { get; set; }

        public CardModel(string ownerName, string cardNumber, string expirationDate, string crypto, string description ="")
        {
            this.ownerName = ownerName;
            this.expirationDate = expirationDate;
            this.crypto = crypto;
            this.cardNumber = cardNumber;
            this.description = description;
        }
    }
}