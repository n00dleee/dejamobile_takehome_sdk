using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dejamobile_takehome_sdk;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitTestSdk
{
    [TestClass]
    public class CardsUnitTests
    {
        Sdk sdk = new Sdk();

        public void init()
        {
            Task<TaskResult> temp = sdk.CreateUser(UsersUnitTests.testUser, UsersUnitTests.testUserPassword);

            temp = sdk.ConnectUser(UsersUnitTests.testUser, UsersUnitTests.testUserPassword);
            TaskResult result = temp.Result;
        }

        [TestMethod]
        public void AddCard()
        {
            init();

            //act
            Task<TaskResult> temp = sdk.AddCard("nicolas debeaupte", "4143869183957495", "01/25", "123");
            TaskResult result = temp.Result;

            //ASSERTS
            //ensure result if true
            Assert.IsTrue(result.result);
            //ensure payload is CardModel typed
            Assert.IsTrue(result.payload.GetType() == typeof(dejamobile_takehome_sdk.Models.CardModel));
            //ensure card ownername is not empty
            Assert.IsTrue( ((dejamobile_takehome_sdk.Models.CardModel)result.payload).ownerName.Length > 0);
            //ensure crypto is contains 3 digits
            Assert.IsTrue(((dejamobile_takehome_sdk.Models.CardModel)result.payload).crypto.Length == 3);
            //ensure cardnumber contains 16 digits
            Assert.IsTrue(((dejamobile_takehome_sdk.Models.CardModel)result.payload).cardNumber.Length == 16);
            //ensure expirationdate contains 5 digits and that a '/' is on pos 2 of the string
            Assert.IsTrue(((dejamobile_takehome_sdk.Models.CardModel)result.payload).expirationDate.Length == 5 && ((dejamobile_takehome_sdk.Models.CardModel)result.payload).expirationDate.Substring(2,1) == "/");
        }

        [TestMethod]
        public void getCardList()
        {
            //arrange
            AddCard();

            //act
            TaskResult result = sdk.getDigitizedCardsList();

            //ASSERTS
            Assert.IsTrue(result.result);
            //ensure payload is List<CardModel> typed
            Assert.IsTrue(result.payload.GetType() == typeof(List<dejamobile_takehome_sdk.Models.CardModel>));
            //ensure payload contains card(s)
            List<dejamobile_takehome_sdk.Models.CardModel> cardList = (List<dejamobile_takehome_sdk.Models.CardModel>)result.payload;
            Assert.IsTrue(cardList.Count > 0);
        }

        [TestMethod]
        public void deleteCard()
        {
            //arrange
            AddCard();

            //act
            TaskResult result = sdk.getDigitizedCardsList();
            List<dejamobile_takehome_sdk.Models.CardModel> cardList = (List<dejamobile_takehome_sdk.Models.CardModel>)result.payload;
            //delete first card in list
            string originalUidToDelete = cardList[0].uid;
            result = sdk.deleteDigitizedCard(originalUidToDelete);

            //ASSERTS
            Assert.IsTrue(result.result);

            //ensure ensure the card linked to the previous uid no longer exists in database
            result = sdk.getDigitizedCardsList();
            cardList = (List<dejamobile_takehome_sdk.Models.CardModel>)result.payload;
            foreach(dejamobile_takehome_sdk.Models.CardModel c in cardList)
            {
                if (c.uid == originalUidToDelete)
                    throw new AssertFailedException();
            }

            //ensure deleting request for this uid fail
            result = sdk.deleteDigitizedCard(originalUidToDelete);
            Assert.IsFalse(result.result);
        }
    }
}
