using System;
using System.Collections.Generic;
using System.Text;
using dejamobile_takehome_sdk.Models;

namespace dejamobile_takehome_sdk.Services.DatabaseManager
{
    class VolatileFakeDigitizedCardDataBase : IDatabaseManager
    {
        bool _isConnected = false;
        List<Models.DigitizedCardModel> digitizedCardList;

        public bool isConnected
        {
            get { return _isConnected; }
        }

        public VolatileFakeDigitizedCardDataBase()
        {
            digitizedCardList = new List<DigitizedCardModel>();
        }

        public bool deleteDigitizedCard(DigitizedCardModel digitizedCard)
        {
            try
            {
                digitizedCardList.Remove(digitizedCard);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<DigitizedCardModel> getDigitizedCardList()
        {
            return digitizedCardList;
        }

        public bool storeDigitizedCard(DigitizedCardModel digitizedCard)
        {
            try
            {
                digitizedCardList.Add(digitizedCard);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool connect()
        {
            _isConnected = true;
            return true;
        }
    }
}
