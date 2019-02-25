using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Models
{
    public class DigitizedCardModel
    {
        public string ownerName { get; set; }
        public string expirationDate { get; set; }
        public string crypto { get; set; }
        public string productionDate { get; set; }
        public string description { get; set; }
    }

    public class CardModel
    {
        public string ownerName { get; set; }
        public string expirationDate { get; set; }
        public string crypto { get; set; }

        public CardModel(string ownerName, string expirationDate, string crypto)
        {
            this.ownerName = ownerName;
            this.expirationDate = expirationDate;
            this.crypto = crypto;
        }
    }
}