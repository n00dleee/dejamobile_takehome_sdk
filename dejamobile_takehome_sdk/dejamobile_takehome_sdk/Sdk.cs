using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace dejamobile_takehome_sdk
{
    public class Sdk
    {
        clientStatus status;
        private DejaMobileHttpClient customHttpClient;
        private Services.DatabaseManager.IDatabaseManager dbManager;

        public Sdk()
        {
            status = clientStatus.unknown;
            customHttpClient = new DejaMobileHttpClient();
            dbManager = new Services.DatabaseManager.VolatileFakeDigitizedCardDataBase();
        }

        private bool init()
        {
            return dbManager.connect();
        }

        private void onUserConnected()
        {
            status = clientStatus.connected;
        }

        private void onUserNotConnected()
        {
            status = clientStatus.disconnected;

        }

        public string getStatus()
        {
            return status.ToString();
        }

        public async Task<bool> CreateUser(string user, string password)
        {
            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.createUser, new Models.UserModel(user, password));
            if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ConnectUser(string user, string password)
        {
            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.logUser, new Models.UserModel(user, password));
            if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> AddCard(string ownerName, string cardNumber, string expDate, string crypto)
        {
            if (status != clientStatus.connected)
                return false; //ERROR not authorized to implement

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.addCard, new Models.CardModel(ownerName, expDate, crypto));
            if (rsp.StatusCode != System.Net.HttpStatusCode.Created)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Models.DigitizedCardModel> getDigitizedCardsList()
        {
            if (status != clientStatus.connected)
                return null; //ERROR not authorized to implement

            if (dbManager.isConnected != true)
                return null; //ERROR db not found to implement

            return dbManager.getDigitizedCardList();
        }

        public bool deleteDigitizedCard(Models.DigitizedCardModel digitizedCard)
        {
            if (status != clientStatus.connected)
                return false; //ERROR not authorized to implement

            if (dbManager.isConnected != true)
                return false; //ERROR db not found to implement

            return dbManager.deleteDigitizedCard(digitizedCard);
        }

        public async Task<bool> getPaymentsHistory()
        {
            if (status != clientStatus.connected)
                return false; //ERROR not authorized to implement

            HttpResponseMessage rsp = await customHttpClient.performRequest(DejaMobileHttpClient.Request.getStats, null);
            if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public enum clientStatus { unknown, disconnected, connected }
    public class DejaMobileHttpClient
    {
        HttpClient httpClient;

        public DejaMobileHttpClient()
        {

        }

        public async Task<HttpResponseMessage> performRequest(Request requestType, Object payload)
        {
            // Serialize our concrete class into a JSON String
            var stringPayload = JsonConvert.SerializeObject(payload);

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            switch (requestType)
            {
                case Request.createUser:
                    //post method
                    response = await httpClient.PostAsync(ApiUrl.user, httpContent);
                    response.EnsureSuccessStatusCode();
                    break;
                case Request.logUser:
                    response = await
                    break;
                default:
                    break;
            }


            return response;
        }

        public enum Request {
            createUser,
            logUser,
            addCard,
            getStats
        }

        public class ApiRequest
        {
            private string baseUrl;
            private string urlSuffix;
            private string urlComplete;
            private HttpMethod method;

            public ApiRequest(Request requestType)
            {
                buildRequest(requestType);
            }

            public HttpMethod getMethod()
            {
                return this.method;
            }

            public string getUrl()
            {
                return this.urlComplete;
            }

            private void buildRequest(Request requestType)
            {
                switch (requestType)
                {
                    case Request.createUser:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.users);
                        method = getHttpMethodFromRequestType(requestType);
                        break;
                    case Request.logUser:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.users);
                        method = getHttpMethodFromRequestType(requestType);
                        break;
                    case Request.addCard:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.digitizedCard);
                        method = getHttpMethodFromRequestType(requestType);
                        break;
                    case Request.getStats:
                        urlComplete = buildCompleteUrl(Models.DejamobileApiModel.statistics);
                        method = getHttpMethodFromRequestType(requestType);
                        break;
                    default:
                        break;
                }

            }

            private string buildCompleteUrl(string suffix)
            {
                return Models.DejamobileApiModel.backendBaseUrl + suffix;
            }

            private HttpMethod getHttpMethodFromRequestType(Request requestType)
            {
                switch (requestType)
                {
                    case Request.createUser:
                        return HttpMethod.Post;
                    case Request.logUser:
                        return HttpMethod.Post;
                    case Request.addCard:
                        return HttpMethod.Post;
                    case Request.getStats:
                        return HttpMethod.Get;
                    default:
                        throw new Exception("Exception in getHttpMethodFromRequestType in switch case, request type is not handled yet : " + requestType.ToString());
                }
            }
        }
    }
}