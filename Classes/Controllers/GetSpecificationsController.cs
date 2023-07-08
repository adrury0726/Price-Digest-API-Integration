using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//This class is being created in order to do a call for when a user selects the configuration they want details for, this will insert all of the specifications into their appropriate tables
//so that we can display it in Vision

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
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {

                Specifications ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                int assetNum = 0;
                userNum = appBody.userNum;
                string callType = "GetSpecificationsController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //Reusing this for the configurationID
                SpecsRequest specsRequest = new GetSpecsBuilder(dBConnection).Build(subscriberId, applicationNum);


                //This is the endpoint we're going to use for this API call
                string endpoint = "specs/basic/?configurationId=" + specsRequest.ConfigurationID;

                //This is where the call is being created
                ret = apiCalls.MakeGet<Specifications>(callId, subscriberId, endpoint, typeof(Specifications));


                if (ret != null)
                {
                    foreach (SpecDetails con in ret.Specs)
                    {
                        //We need this to be able to accept nulls. SpecValue is the only variable that can return nulls 
                        object specValue = con.SpecValue != null ? (object)con.SpecValue : DBNull.Value;

                        dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestGrabSpecificationsAPICall",

                            //Model properties
                            new object[] { applicationNum, subscriberId, assetNum, specsRequest.ConfigurationID, con.SpecName, specValue, con.SpecNameFriendly, con.SpecUom, con.SpecDescription, con.SpecFamily },

                            //Sprocs variables
                            new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationId", "SpecName", "SpecValue", "SpecNameFriendly", "SpecUom", "SpecDescription", "SpecFamily" },

                            //Datatypes
                            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar });

                    }
                }
                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                //return the list if you want to (probably not going to be how info is returned to vision. My guess is you'll always update a table which vision will pull from. That means all your calls are probably going to be puts and posts)
                return new OkResult();
            }
            catch (Exception ex)
            {
                callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("Invalid input. Check that the data is correct in the form."); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
