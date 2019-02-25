using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dejamobile_takehome_sdk
{
    public class Sdk
    {
        bool status;
        private DejaMobileHttpClient customHttpClient;
        private Services.DatabaseManager.IDatabaseManager dbManager;

        public Sdk()
        {
            status = false;
            customHttpClient = new DejaMobileHttpClient();
            dbManager = new Services.DatabaseManager.VolatileFakeDigitizedCardDataBase();
        }

        private TaskResult init()
        {
            dbManager.connect();

            if (dbManager.isConnected)
            {
                return new TaskResult(true, TaskResult.TaskStatus.finished, null,"SDK is ready");
            }
            else
            {
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is unreachable. Please ensure Database is running");
            }
        }

        private void onUserConnected()
        {
            status = true;
        }

        private void onUserNotConnected()
        {
            status = false;

        }

        public bool getStatus()
        {
            return status;
        }

        public async Task<TaskResult> CreateUser(string userName, string password, string firstName ="", string lastName = "", string phoneNumber = "")
        {
            try
            {
                HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.createUser, new Models.UserModel(userName, password, firstName, lastName, phoneNumber));
                if (rsp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new TaskResult(true, TaskResult.TaskStatus.finished, null, "User successfully created");
                }
                else
                {
                    return new TaskResult(false, TaskResult.TaskStatus.finished, null, "ERROR while creating user : " + rsp.StatusCode.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "ERROR internal SDK exception while processing CREATE USER request");
            }

        }

        public async Task<TaskResult> ConnectUser(string userName, string password)
        {
            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.logUser, new Models.UserModel(userName, password));
            if (rsp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                onUserConnected();
                return new TaskResult(true, TaskResult.TaskStatus.finished, null, "User successfully connected");
            }
            else
            {
                onUserNotConnected();
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "ERROR while connecting user : " + rsp.StatusCode.ToString());
            }
        }

        public async Task<TaskResult> AddCard(string ownerName, string cardNumber, string expDate, string crypto)
        {
            if (!status)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.addCard, new Models.CardModel(ownerName, expDate, crypto));
            if (rsp.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return new TaskResult(true, TaskResult.TaskStatus.finished, null, "Card successfully added"); //TODO : add card should allow sdk to get a digitized card
            }
            else
            {
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to add a card"); //TODO : specific error handler
            }
        }

        public TaskResult getDigitizedCardsList()
        {
            if (!status)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            List<Models.DigitizedCardModel> cardList = dbManager.getDigitizedCardList();
            if (cardList != null)
                return new TaskResult(true, TaskResult.TaskStatus.finished, cardList, "Here is the list of locally stored digitized cards");
            else
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to get digitized cards");
        }

        public TaskResult deleteDigitizedCard(Models.DigitizedCardModel digitizedCard)
        {
            if (!status)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            if (dbManager.deleteDigitizedCard(digitizedCard))
                return new TaskResult(true, TaskResult.TaskStatus.finished, null, "Card successfully deleted");
            else
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while deleting digitized card");
        }

        public async Task<TaskResult> getPaymentsHistory()
        {
            if (!status)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.getStats, null);
            if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new TaskResult(true, TaskResult.TaskStatus.finished, null, "Here is the payment history for the connected user");
            }
            else
            {
                return new TaskResult(false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to get payment history : " + rsp.StatusCode);
            }
        }
    }

    public enum clientStatus { unknown, disconnected, connected }

    public class DejaMobileHttpClient
    {
        HttpClient httpClient;

        public DejaMobileHttpClient()
        {
            httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> performRequest(Request requestType, Object payload)
        {
            // Serialize our concrete class into a JSON String
            var stringPayload = JsonConvert.SerializeObject(payload);

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            ApiRequest request = new ApiRequest(requestType);
            bool result;
            switch (request.getMethod()) //HttpMethod collection is not considered as "constant", cannot switch on it :(
            {
                case Method.post:
                    response = await httpClient.PostAsync(request.getUrl(), httpContent);
                    result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                    //TODO Clarify who handles the error
                    return response;
                case Method.delete:
                    response = await httpClient.DeleteAsync(request.getUrl());
                    result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                    //TODO Clarify who handles the error
                    return response;
                case Method.get: //default case will be GET method
                default:
                    response = await httpClient.GetAsync(request.getUrl());
                    result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                    //TODO Clarify who handles the error
                    return response;
            }
        }

        public enum Request {
            createUser,
            logUser,
            addCard,
            getStats
        }

        public enum Method // Net.HttpMethod objects are not considered as constant and cannot be switched on :( This enum solves this issue
        {
            post,
            get,
            delete,
            put,
            patch
        }

        public class ApiRequest
        {
            private string baseUrl;
            private string urlSuffix;
            private string urlComplete;
            private Method method; // "Method" type from custom enum is used instead of Net.method
            private System.Net.HttpStatusCode expectedStatusCode;

            public ApiRequest(Request requestType)
            {
                buildRequest(requestType);
            }

            public Method getMethod()
            {
                return this.method;
            }

            public string getUrl()
            {
                return this.urlComplete;
            }

            public System.Net.HttpStatusCode getExpectedStatusCode()
            {
                return expectedStatusCode;
            }

            private void buildRequest(Request requestType)
            {
                switch (requestType)
                {
                    case Request.createUser:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.users);
                        method = getHttpMethodFromRequestType(requestType);
                        expectedStatusCode = System.Net.HttpStatusCode.Created;
                        break;
                    case Request.logUser:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.login);
                        method = getHttpMethodFromRequestType(requestType);
                        expectedStatusCode = System.Net.HttpStatusCode.OK;
                        break;
                    case Request.addCard:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.digitizedCard);
                        method = getHttpMethodFromRequestType(requestType);
                        expectedStatusCode = System.Net.HttpStatusCode.Created;
                        break;
                    case Request.getStats:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.statistics);
                        method = getHttpMethodFromRequestType(requestType);
                        expectedStatusCode = System.Net.HttpStatusCode.OK;
                        break;
                    default:
                        break;
                }

            }

            private string buildCompleteUrl(string suffix)
            {
                return Models.DejamobileApiModel.backendBaseUrl + suffix;
            }

            private Method getHttpMethodFromRequestType(Request requestType)
            {
                switch (requestType)
                {
                    case Request.createUser:
                        return Method.post;
                    case Request.logUser:
                        return Method.post;
                    case Request.addCard:
                        return Method.post;
                    case Request.getStats:
                        return Method.get;
                    default:
                        throw new Exception("Exception in getHttpMethodFromRequestType in switch case, request type is not handled yet : " + requestType.ToString());
                }
            }

            public bool ensureStatusCodeMatchesExpectedOne(HttpStatusCode receivedCode)
            {
                if (receivedCode != expectedStatusCode)
                    return false;
                else
                    return true;
            }
        }
    }

    public class TaskResult
    {
        public bool result;
        public TaskStatus status;
        public object payload;
        public string message;

        public enum TaskStatus { pending, finished }

        public TaskResult(bool result, TaskStatus status = TaskStatus.finished, object payload = null, string message = "")
        {
            this.result = result;
            this.status = status;
            this.payload = payload;
            this.message = message;
        }
    }
}