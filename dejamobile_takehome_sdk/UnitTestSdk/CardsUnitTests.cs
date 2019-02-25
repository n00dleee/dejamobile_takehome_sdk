using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dejamobile_takehome_sdk;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnitTestSdk
{
    [TestClass]
    public class CardsUnitTests
    {
        [TestMethod]
        public void AddCard()
        {
            //arrange
            Sdk sdk = new Sdk();
            Task<TaskResult> temp = sdk.ConnectUser(UsersUnitTests.testUser, UsersUnitTests.testUserPassword);
            TaskResult result = temp.Result;
            Assert.IsTrue(result.result);

            //act
            temp = sdk.AddCard("nicolas debeaupte", "1111222333444", "01/25", "123");
            result = temp.Result;

            //assert
            Assert.IsTrue(result.result);
        }
    }
}
