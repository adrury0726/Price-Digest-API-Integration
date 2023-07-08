using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//This class was created in order to grab all of the incomplete truck info from the IncompleteTruckBuildout form in Vision so we can plug in that info and get the values for it.

namespace PriceDigestAPI
{
    [Route("PostIncompleteTruck")]
    [ApiController]
    public class GetIncompleteTruckController : ControllerBase
    {
        [HttpPost(Name = "PostIncompleteTruck")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {
                List<Configurations> ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                int assetNum = 0;
                userNum = appBody.userNum;
                string callType = "GetIncompleteTruckController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //Using this builder in order to grab the Category, ModelName, and ModelYear saved in Vision for the endpoint
                IncompleteTruckRequest truckRequest = new GetIncompleteTruckBuilder(dBConnection).Build(subscriberId, applicationNum);


                //This is our endpoint 
                string endpoint =
                "details/configurations/?manufacturer=" + truckRequest.Category + "&model=" + truckRequest.ModelName + "&year=" + truckRequest.ModelYear;


                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGetListObject<Configurations>(callId, subscriberId, endpoint, typeof(Configurations));


                if (ret != null)
                {
                    foreach (Configurations con in ret)
                    {
                        //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                        dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestGetTruckBodyBuildConfigAPICall",

                                 //Model properties
                                 new object[] { applicationNum, subscriberId, con.ConfigurationId },

                                 //Sproc Variables
                                 new string[] { "ApplicationNum", "SubscriberID", "ConfigurationId" },

                                 //You guessed it, data types
                                 new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int });


                        foreach (DisplaySpec spec in con.DisplaySpecs)
                        {

                            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestGetSelectedTruckBodyValuesAPICall",

                                //Model properties
                                new object[] { applicationNum, subscriberId, spec.SpecName, spec.SpecValue, spec.SpecNameFriendly, spec.SpecUom, spec.SpecDescription, spec.SpecFamily, con.ConfigurationId },

                                //Sproc Variables
                                new string[] { "ApplicationNum", "SubscriberID", "SpecName", "SpecValue", "SpecNameFriendly", "SpecUom", "SpecDescription", "SpecFamily", "ConfigurationId" },

                                //You guessed it, data types
                                new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int });
                        }
                    }
                }
                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                return new OkResult();
            }
            catch (Exception ex)
            {
                callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("Invalid input. Check that the data is correct in the form."); // Usually due to endpoint variables not being present
            }
        }
    }
}
