using System.Data;
using RestSharp;

namespace PriceDigestAPI.Classes
{
    public interface ILogging
    {
        long? startCall(int subscriberId, int applicationNum, int userNum, string callType, string db);
        void endCall(int subscriberId, int applicationNum, int userNum, long? callId);
        void logCall(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, List<Parameter> requestHeaders = null, List<Parameter> responseHeaders = null, long? runTimeInMS = null);
        void logCallWithHeaders(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, ParametersCollection requestHeaders = null, IReadOnlyCollection<HeaderParameter> responseHeaders = null, long? runTimeInMS = null);
        void logError(int subscriberId, int applicationNum, int userNum, long? callId, Exception e);
    }
    //Placeholder logging
    public class DummyCallLogging : ILogging
    {
        public void endCall(int subscriberId, int applicationNum, int userNum, long? callId) { }
        public void logCall(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, List<Parameter> requestHeaders = null, List<Parameter> responseHeaders = null, long? runTimeInMS = null) { }
        public void logCallWithHeaders(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, ParametersCollection requestHeaders = null, IReadOnlyCollection<HeaderParameter> responseHeaders = null, long? runTimeInMS = null) { }
        public void logError(int subscriberId, int applicationNum, int userNum, long? callId, Exception e) { }
        public long? startCall(int subscriberId, int applicationNum, int userNum, string callType, string db) { return -1; }
    }
    public class CallLogging : ILogging
    {
        private IDBConnection dBConnection;
        public CallLogging(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }
        private string getHeaders(List<Parameter> headers)
        {
            if (headers != null && headers.Count > 0)
            {
                string headersString = "";
                foreach (Parameter param in headers)
                {
                    if (param.Type.ToString() == "HttpHeader")
                    {
                        headersString += $"-H \"{param.Name.ToString()}: {param.Value.ToString()}\" \n";
                    }
                }
                return headersString;
            }
            else return null;
        }
        //The initial call for each request.
        public long? startCall(int subscriberId, int applicationNum, int userNum, string callType, string db)
        {
            long? callId;
            DataTable dt = new DataTable();

            if (db == "Tandem")
                db = "TandemVision";
            else if (db == "Pawnee")
                db = "PawneeVision";
            dBConnection.setupConnection(db);

            dBConnection.ExecSproc(ref dt, "Sub" + subscriberId + ".sPriceDigestGenCallId", new object[] { subscriberId, applicationNum, userNum, callType }, new string[] { "SubscriberID", "ApplicationNum", "UserNum", "CallType" }, new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar });
            //add this callId to all logging
            callId = PDUtility.nullToNullLong(dt.Rows[0]["CallId"]);

            //PD = Price Digest
            logCall(callId, subscriberId, $"START - SendtoPD({subscriberId},{applicationNum},{userNum})");

            return callId;
        }
        public void endCall(int subscriberId, int applicationNum, int userNum, long? callId)
        {
            //this is to make sure that the last call always gets logged before the call that says "END - ..."
            Thread.Sleep(10);

            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestEndCall", new object[] { callId, 1 }, new string[] { "CallId", "CallStatus" }, new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            logCall(callId, subscriberId, $"END - SendtoPD({subscriberId},{applicationNum},{userNum})");
            dBConnection.destroyConnection();
        }
        public void logCall(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, List<Parameter> requestHeaders = null, List<Parameter> responseHeaders = null, long? runTimeInMS = null)
        {
            string curl = null;
            string requestHeadersString = getHeaders(requestHeaders);
            string responseHeadersString = getHeaders(responseHeaders);
            if (requestUrl != null)
            {
                //build out the curl
                curl = $"curl --location --request {method} {requestUrl}  {requestHeadersString}";
                if (requestBody != null) curl += $"-d \"{requestBody.Replace("\"", "\\\"")}\"";
            }
            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestLogging", new object[] { callId, message, curl, requestUrl, method, requestBody, response, requestHeadersString, responseHeadersString, runTimeInMS }, new string[] { "CallId", "CustomMessage", "curl", "RequestUrl", "Method", "RequestBody", "Response", "RequestHeaders", "ResponseHeaders", "RunTimeInMS" }, new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int });
        }
        public void logCallWithHeaders(long? callId, int subscriberId, string message, string requestUrl = null, string method = null, string requestBody = null, string response = null, ParametersCollection requestHeaders = null, IReadOnlyCollection<HeaderParameter> responseHeaders = null, long? runTimeInMS = null)
        {
            List<Parameter> responseHeads = new List<Parameter>();
            if (responseHeaders != null)
            {
                foreach (HeaderParameter header in responseHeaders)
                {
                    responseHeads.Add(header);
                }
            }

            List<Parameter> requestHeads = new List<Parameter>();
            if (requestHeaders != null)
            {
                foreach (Parameter header in requestHeaders)
                {
                    requestHeads.Add(header);
                }
            }

            logCall(callId, subscriberId, message, requestUrl, method, requestBody, response, requestHeads, responseHeads, runTimeInMS);
        }
        public void logError(int subscriberId, int applicationNum, int userNum, long? callId, Exception e)
        {
            //we're gonna want to beef up this logging eventually. this is just a start
            string error = $"ERROR - SendtoPD({subscriberId},{applicationNum},{userNum}) - {e.Message} - {e.StackTrace} - {e.InnerException} - {e.GetBaseException()}";
            Console.WriteLine(error);
            logCall(callId, subscriberId, error);
            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestEndCall", new object[] { callId, 2 }, new string[] { "CallId", "CallStatus" }, new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });
            dBConnection.destroyConnection();
        }
    }
}
