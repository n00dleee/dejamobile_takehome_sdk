using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dejamobile_takehome_sdk;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnitTestSdk
{
    [TestClass]
    public class UsersUnitTests
    {
        Sdk sdk = new Sdk(true);
        public static string testUser = "testUser";
        public static string testUserPassword = "testPassword";
        dejamobile_takehome_sdk.Models.UserModel user;

        [TestMethod]
        public void CreateUser()
        {
            //arrange
            user = new dejamobile_takehome_sdk.Models.UserModel(UsersUnitTests.testUser, UsersUnitTests.testUserPassword);
            //act
            Task<TaskResult> temp = sdk.CreateUser(user);
            TaskResult result = temp.Result;

            //assert
            Assert.IsTrue(result.result);
        }

        [TestMethod]
        public void LogTestUser()
        {
            //arrange
            CreateUser();
            user = new dejamobile_takehome_sdk.Models.UserModel(UsersUnitTests.testUser, UsersUnitTests.testUserPassword);

            //act
            Task<TaskResult> temp = sdk.ConnectUser(user);
            TaskResult result = temp.Result;

            //ASSERTS
            Assert.IsTrue(result.result);

            //ensure user is properly connected
            Assert.IsTrue(sdk.getStatus());
        }

        [TestMethod]
        public void TestRefreshToken()
        {
            string expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE1NTEyNzIzNzMsImV4cCI6MTU1MTI3MjM3M30.lqMpYLekfiCA9niarNFDW35evHoBzhmNEYgLrmNvsxU";

            //arrange
            CreateUser();
            sdk.injectThisToken(new dejamobile_takehome_sdk.Models.UserModel(testUser, testUserPassword), expiredToken);

            sdk.init();

            //act
            Task<TaskResult> temp = sdk.AddCard("nicolas debeaupte", "4143869183957495", "01/25", "123");
            TaskResult result = temp.Result;
            Assert.IsTrue(result.result);
        }
    }
}
