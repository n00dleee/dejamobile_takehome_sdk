using System;
using System.Collections.Generic;
using System.Text;
using dejamobile_takehome_sdk.Models;

namespace dejamobile_takehome_sdk.Services.DatabaseManager
{
    class VolatileFakeDigitizedCardDataBase : IDatabaseManager
    {
        bool _isConnected = false;
        List<Models.CardModel> digitizedCardList;

        public bool isConnected
        {
            get { return _isConnected; }
        }

        public VolatileFakeDigitizedCardDataBase()
        {
            digitizedCardList = new List<CardModel>();
        }

        public bool deleteDigitizedCard(string uid)
        {
            foreach(Models.CardModel card in digitizedCardList)
            {
                if(card.uid == uid)
                {
                    digitizedCardList.Remove(card);
                    return true;
                }
            }

            //if we reach this line, uid was not found
            return false;
        }

        public List<CardModel> getDigitizedCardList()
        {
            return digitizedCardList;
        }

        public bool storeDigitizedCard(CardModel digitizedCard)
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
