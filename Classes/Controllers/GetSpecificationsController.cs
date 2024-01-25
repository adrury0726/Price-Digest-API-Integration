using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

namespace PriceDigestAPI
{
    [Route("PostSpecifications")]
    [ApiController]
    public class GetSpecificationsController : ControllerBase
    {
        [HttpPost(Name = "PostSpecifications")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            Specifications ret = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int assetNum = 0;
            int userNum = appBody.userNum;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //Reusing this for the configurationID
            SpecsRequest specsRequest = new GetSpecsBuilder(dBConnection).Build(subscriberId, applicationNum);


            //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using 
            string endpoint = "specs/basic/?configurationId=" + specsRequest.ConfigurationID;

            //actually make the get (call the price digest api)
            
            ret = apiCalls.MakeGet<Specifications>(callId, subscriberId, endpoint, typeof(Specifications));


            if (ret != null)
                    {
                        foreach (SpecDetails con in ret.Specs)
                        {
                            //We need this to be able to accept nulls.
                            object specValue = con.SpecValue != null ? (object)con.SpecValue : DBNull.Value;

                            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestGrabSpecificationsAPICall",
                                new object[] { applicationNum, subscriberId, assetNum, specsRequest.ConfigurationID, con.SpecName, specValue, con.SpecNameFriendly, con.SpecUom, con.SpecDescription, con.SpecFamily},

                                new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationId", "SpecName", "SpecValue", "SpecNameFriendly", "SpecUom", "SpecDescription", "SpecFamily" },

                             new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar});

                        }
                    }

            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            //return the list if you want to (probably not going to be how info is returned to vision. My guess is you'll always update a table which vision will pull from. That means all your calls are probably going to be puts and posts)
            return new OkResult();
        }
    }
}
