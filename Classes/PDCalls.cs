using RestSharp;
using Newtonsoft.Json;
using System.Diagnostics;
using PriceDigestAPI.Classes;

namespace PriceDigestAPI
{
    public interface IAPICalls
    {
        void MakePost(long? callId, int subscriberId, object model, string endPoint);
        List<T> MakePostReturnsList<T>(long? callId, int subscriberId, object model, string endPoint, Type type);
        void MakePut(long? callId, int subscriberId, object model, string endPoint);
        dynamic MakeGet<T>(long? callId, int subscriberId, string endPoint, Type type);
        List<T> MakeGetListObject<T>(long? callId, int subscriberId, string endPoint, Type type);
    }
    public class PDCalls : IAPICalls
    {
        private ILogging logging;
        private IDBConnection dBConnection;
        private IConfiguration configuration;
        public PDCalls(ILogging logging, IDBConnection dBConnection, IConfiguration configuration)
        {
            this.logging = logging;
            this.dBConnection = dBConnection;
            this.configuration = configuration;
        }
        public void MakePost(long? callId, int subscriberId, object model, string endPoint)
        {
            MakeCall(callId, subscriberId, endPoint, Method.Post, model);
        }
        public List<T> MakePostReturnsList<T>(long? callId, int subscriberId, object model, string endPoint, Type type)
        {
            string ret = MakeCall(callId, subscriberId, endPoint, Method.Post, model);

            List<T> retModels = JsonConvert.DeserializeObject<List<T>>(ret);

            return retModels;
        }

        public void MakePut(long? callId, int subscriberId, object model, string endPoint)
        {
            MakeCall(callId, subscriberId, endPoint, Method.Put, model);
        }
        public dynamic MakeGet<T>(long? callId, int subscriberId, string endPoint, Type type)
        {
            //I'm trusting anyone who calls this to give me an object that matches the given endpoint
            string ret = MakeCall(callId, subscriberId, endPoint, Method.Get);


            dynamic retModel = JsonConvert.DeserializeObject<T>(ret);

            return retModel;

        }
        public List<T> MakeGetListObject<T>(long? callId, int subscriberId, string endPoint, Type type)
        {
            //I'm trusting anyone who calls this to give me an object that matches the given endpoint
            string ret = MakeCall(callId, subscriberId, endPoint, Method.Get);

            List<T> retModels = JsonConvert.DeserializeObject<List<T>>(ret);

            return retModels;
        }
        public void MakeDelete(long? callId, int subscriberId, string endPoint)
        {
            MakeCall(callId, subscriberId, endPoint, Method.Delete);
        }
        public void MakeDelete(long? callId, int subscriberId, object model, string endPoint)
        {
            MakeCall(callId, subscriberId, endPoint, Method.Delete, model);
        }
        private string MakeCall(long? callId, int subscriberId, string endPoint, Method method, object model = null)
        {

            var request = new RestRequest();
            //var baseUrl = dBConnection.LookupItemData(subscriberId, 0, "PriceDigestIntegration", "BaseUrl");
            //var version = dBConnection.LookupItemData(subscriberId, 0, "PriceDigestIntegration", "Version");
            //var apiKey = dBConnection.LookupItemData(subscriberId, 0, "PriceDigestIntegration", "ApiKey");
            //var username = "todo"//dBConnection.LookupItemData(subscriberId, 0, "PriceDigestIntegration", "UserName");
            //var password = "todo"//dBConnection.LookupItemData(subscriberId, 0, "PriceDigestIntegration", "Password");
            //var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            //var token = Convert.ToBase64String(byteArray);//this is an encoding of the username and password

            string baseUrl = configuration.GetValue<string>("PriceDigestURL");
            var version = configuration.GetValue<string>("Version");
            var apiKey = configuration.GetValue<string>("ApiKey");

            string fullUrl = $"{baseUrl}/{version}/{endPoint}";

            string json = null;

            if (model != null)
            {
                json = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                request.AddStringBody(json, "application/json");
                //request.AddBody(json);
            }

            request.AddHeader("x-api-key", apiKey);
            request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("Authorization", $"Basic {token}");
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("Content-Type", "application/json", ParameterType.HttpHeader);

            var client = new RestClient(fullUrl);

            Stopwatch s = new Stopwatch();
            s.Start();
            RestResponse response = client.ExecuteAsync(request, method).Result;
            s.Stop();

            logging.logCallWithHeaders(callId, subscriberId, null, client.Options.BaseUrl.ToString(), method.ToString(), json, response.Content, request.Parameters, response.Headers, s.ElapsedMilliseconds);

            return response.Content;
        }
    }
}
