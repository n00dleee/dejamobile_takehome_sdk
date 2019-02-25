using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk.Services.DatabaseManager
{
    interface IDatabaseManager
    {
        bool isConnected { get; }
        bool connect();
        bool storeDigitizedCard(Models.DigitizedCardModel digitizedCard);
        List<Models.DigitizedCardModel> getDigitizedCardList();
        bool deleteDigitizedCard(Models.DigitizedCardModel digitizedCard);
    }
}
