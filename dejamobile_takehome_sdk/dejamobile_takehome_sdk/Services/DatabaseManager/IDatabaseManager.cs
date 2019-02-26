using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Services.DatabaseManager
{
    interface IDatabaseManager
    {
        bool isConnected { get; }
        bool connect();
        bool storeDigitizedCard(Models.CardModel digitizedCard);
        List<Models.CardModel> getDigitizedCardList();
        bool deleteDigitizedCard(string uid);
    }
}
