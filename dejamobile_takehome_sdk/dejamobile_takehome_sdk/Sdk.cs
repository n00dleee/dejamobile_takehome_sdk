using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace dejamobile_takehome_sdk
{
    public class Sdk
    {
        bool status;
        private DejaMobileHttpClient customHttpClient;
        private Services.DatabaseManager.IDatabaseManager dbManager;
        private Models.UserModel currentUser;
        private Config config;

        public Sdk(bool autoReconnectOnTokenExpiration)
        {
            status = false;
            config = new Config(autoReconnectOnTokenExpiration);
            customHttpClient = new DejaMobileHttpClient();
            dbManager = new Services.DatabaseManager.VolatileFakeDigitizedCardDataBase();
            init();
        }

        //TODO remove
        public void injectThisToken(Models.UserModel user, string token)
        {
            customHttpClient.storeAuthJwt(user, token);
            status = true;
        }

        public TaskResult init()
        {
            dbManager.connect();

            if (dbManager.isConnected)
            {
                return new TaskResult(TaskResult.TaskName.startup, true, TaskResult.TaskStatus.finished, null,"SDK is ready");
            }
            else
            {
                return new TaskResult(TaskResult.TaskName.startup, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is unreachable. Please ensure Database is running");
            }
        }

        private void onUserConnected(Models.UserModel currentUser, string token)
        {
            status = true;
            this.currentUser = currentUser;
            customHttpClient.storeAuthJwt(currentUser, token);
        }

        private void onUserNotConnected()
        {
            status = false;

        }

        public bool getStatus()
        {
            return status;
        }

        public async Task<TaskResult> CreateUser(Models.UserModel user)
        {
            try
            {
                HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.createUser, user);
                if (rsp.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return new TaskResult(TaskResult.TaskName.createUser, true, TaskResult.TaskStatus.finished, null, "User successfully created");
                }
                else
                {
                    return new TaskResult(TaskResult.TaskName.createUser, false, TaskResult.TaskStatus.finished, null, "ERROR while creating user : " + rsp.StatusCode.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return new TaskResult(TaskResult.TaskName.createUser, false, TaskResult.TaskStatus.finished, null, "ERROR internal SDK exception while processing CREATE USER request");
            }

        }

        public async Task<TaskResult> ConnectUser(Models.UserModel user)
        {
            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.logUser, user);
            if (rsp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await DejaMobileHttpClient.getJsonFromHttpResponse(rsp);
                Models.AuthJwtModel authJwt = JsonConvert.DeserializeObject<Models.AuthJwtModel>(json);
                onUserConnected(user, authJwt.token);
                return new TaskResult(TaskResult.TaskName.logUser, true, TaskResult.TaskStatus.finished, null, "User successfully connected");
            }
            else
            {
                onUserNotConnected();
                return new TaskResult(TaskResult.TaskName.logUser, false, TaskResult.TaskStatus.finished, null, "ERROR while connecting user : " + rsp.StatusCode.ToString());
            }
        }

        public async Task<TaskResult> AddCard(Models.CardModel card)
        {
            if (!status)
                return new TaskResult(TaskResult.TaskName.addCard, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.addCard, card);
            if (rsp.StatusCode == System.Net.HttpStatusCode.Created)
            {
                string json = await DejaMobileHttpClient.getJsonFromHttpResponse(rsp);
                Models.CardModel digitizedCard = JsonConvert.DeserializeObject<Models.CardModel>(json);
                //add production date & description
                digitizedCard.description = card.description;
                digitizedCard.productionDate = DateTime.Now.ToString();
                digitizedCard.uid = Guid.NewGuid().ToString();

                if(storeCardInDb(digitizedCard))
                    return new TaskResult(TaskResult.TaskName.addCard, true, TaskResult.TaskStatus.finished, digitizedCard, "Card successfully added");
                else
                    return new TaskResult(TaskResult.TaskName.addCard, false, TaskResult.TaskStatus.finished, digitizedCard, "SDK ERROR : Card successfully added but an error has been thrown while trying to store card in database");
            }
            else
            {
                return new TaskResult(TaskResult.TaskName.addCard, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to add a card"); //TODO : specific error handler
            }
        }

        public TaskResult getDigitizedCardsList()
        {
            if (!status)
                return new TaskResult(TaskResult.TaskName.getCards, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(TaskResult.TaskName.getCards, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            List<Models.CardModel> cardList = dbManager.getDigitizedCardList();
            if (cardList != null)
                return new TaskResult(TaskResult.TaskName.getCards, true, TaskResult.TaskStatus.finished, cardList, "Here is the list of locally stored digitized cards");
            else
                return new TaskResult(TaskResult.TaskName.getCards, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to get digitized cards");
        }

        public TaskResult deleteDigitizedCard(string uid)
        {
            if (!status)
                return new TaskResult(TaskResult.TaskName.removeCard, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(TaskResult.TaskName.removeCard, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            if (dbManager.deleteDigitizedCard(uid))
                return new TaskResult(TaskResult.TaskName.removeCard, true, TaskResult.TaskStatus.finished, null, "Card successfully deleted");
            else
                return new TaskResult(TaskResult.TaskName.removeCard, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while deleting digitized card");
        }

        public async Task<TaskResult> getPaymentsHistory()
        {
            if (!status)
                return new TaskResult(TaskResult.TaskName.getHistory, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : user not connected. Please connect user before trying to use this method");

            if (dbManager.isConnected != true)
                return new TaskResult(TaskResult.TaskName.getHistory, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : database is not accessible. All data management is disabled until database is back on track. Please retry to use init() method");

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.getStats, null);
            if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new TaskResult(TaskResult.TaskName.getHistory, true, TaskResult.TaskStatus.finished, null, "Here is the payment history for the connected user");
            }
            else
            {
                return new TaskResult(TaskResult.TaskName.getHistory, false, TaskResult.TaskStatus.finished, null, "SDK ERROR : error while trying to get payment history : " + rsp.StatusCode);
            }
        }

        private bool storeCardInDb(Models.CardModel card)
        {
            return dbManager.storeDigitizedCard(card);
        }

        private bool deleteCardFromDb(string uid)
        {
            return dbManager.deleteDigitizedCard(uid);
        }
    }

    public class DejaMobileHttpClient
    {
        HttpClient httpClient;
        string jwt;
        private Models.UserModel currentUser;

        public DejaMobileHttpClient()
        {
            httpClient = new HttpClient();
        }

        public void storeAuthJwt(Models.UserModel currentUser, string jwt)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            this.jwt = jwt;
            this.currentUser = currentUser;
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
            bool tokenNeedsRefresh;
                switch (request.getMethod()) //HttpMethod collection is not considered as "constant", cannot switch on it :(
                {
                    case Method.post:
                        response = await httpClient.PostAsync(request.getUrl(), httpContent);
                        result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        tokenNeedsRefresh = await doesClientNeedTokenRefresh(response);
                        if (tokenNeedsRefresh)
                        {
                            bool b = await refreshToken();
                            if (b)
                            {
                                response = await retryRequest(request, payload);
                                result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                                return response;
                            }
                            else
                            {
                                return response;
                                //TODO : trigger some kind of event to notify SDK something is wrong with auth
                            }
                        }
                        else
                            return response;
                    case Method.delete:
                        response = await httpClient.DeleteAsync(request.getUrl());
                        result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        tokenNeedsRefresh = await doesClientNeedTokenRefresh(response);
                        if (tokenNeedsRefresh)
                        {
                            bool b = await refreshToken();
                            if (b)
                            {
                                response = await retryRequest(request, httpContent);
                                result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                                return response;
                            }
                            else
                            {
                                return response;
                                //TODO : trigger some kind of event to notify SDK something is wrong with auth
                            }
                        }
                        else
                            return response;
                    case Method.get: //default case will be GET method
                    default:
                        response = await httpClient.GetAsync(request.getUrl());
                        result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        tokenNeedsRefresh = await doesClientNeedTokenRefresh(response);
                        if (tokenNeedsRefresh)
                        {
                            bool b = await refreshToken();
                            if (b)
                            {
                                response = await retryRequest(request, httpContent);
                                result = request.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                                return response;
                            }
                            else
                            {
                                return response;
                                //TODO : trigger some kind of event to notify SDK something is wrong with auth
                            }
                        }
                        else
                            return response;
                }
        }

        private async Task<bool> refreshToken()
        {
            try
            {
                var stringPayload = JsonConvert.SerializeObject(currentUser);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(Models.DejamobileApiModel.backendBaseUrl + Models.DejamobileApiModel.login, httpContent);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string json = await getJsonFromHttpResponse(response);
                    Models.AuthJwtModel authJwt = JsonConvert.DeserializeObject<Models.AuthJwtModel>(json);
                    storeAuthJwt(currentUser, authJwt.token);
                    Console.WriteLine("Token has been refreshed !");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception while trying to refreshtoken : " + e.Message);
                return false;
            }
        }

        private async Task<HttpResponseMessage> retryRequest(ApiRequest apiRequest, Object payload)
        {
            HttpResponseMessage response;
            bool result;

            try
            {
                var stringPayload = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                switch (apiRequest.getMethod())
                {
                    case Method.post:
                        response = await httpClient.PostAsync(apiRequest.getUrl(), httpContent);
                        result = apiRequest.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        return response;

                    case Method.delete:
                        response = await httpClient.DeleteAsync(apiRequest.getUrl());
                        result = apiRequest.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        return response;

                    case Method.get:
                    default:
                        response = await httpClient.DeleteAsync(apiRequest.getUrl());
                        result = apiRequest.ensureStatusCodeMatchesExpectedOne(response.StatusCode);
                        return response;
                }
            }catch(Exception e)
            {
                Console.WriteLine("Exception while trying to retry http request " + e.Message);
                return null;
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

        public static async Task<string> getJsonFromHttpResponse(HttpResponseMessage response)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        private async Task<bool> doesClientNeedTokenRefresh(HttpResponseMessage rsp)
        {
            string json = await getJsonFromHttpResponse(rsp);

            if (rsp.StatusCode == HttpStatusCode.Unauthorized && json.Contains("jwt expired"))
                return true;
            else
                return false;
        }
    }

    public class TaskResult
    {
        public TaskName name;
        public bool result;
        public TaskStatus status;
        public object payload;
        public string message;

        public enum TaskStatus { pending, finished }
        public enum TaskName { createUser, logUser, addCard, removeCard, getCards, getHistory,
            startup
        }

        public TaskResult(TaskName name, bool result, TaskStatus status = TaskStatus.finished, object payload = null, string message = "")
        {
            this.name = name;
            this.result = result;
            this.status = status;
            this.payload = payload;
            this.message = message;
        }
    }
}