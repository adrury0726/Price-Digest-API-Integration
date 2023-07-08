using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This is the class I created in order to get the necessary classifications for our Year/Make/Model drop-downs and calls

namespace PriceDigestAPI
{
    [Route("PostClassifications")]
    [ApiController]
    public class GetClassificationsController : ControllerBase
    {
        [HttpPost(Name = "PostClassifications")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {
                List<Classifications> ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                userNum = 0;
                string callType = "GetClassificationsController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //This is our API call
                string endpoint = "taxonomy/classifications";

                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGetListObject<Classifications>(callId, subscriberId, endpoint, typeof(Classifications));


                if (ret != null)
                {
                    foreach (Classifications con in ret)
                    {

                        //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                        //Model properties
                        dBConnection.ExecSproc("Sub" + subscriberId + ".PriceDigestClassificationsAPICall",
                                 new object[] { con.ClassificationID, con.ClassificationName },

                                 //Sproc Variables
                                 new string[] { "ClassificationID", "ClassificationName" },

                                 //You guessed it, data types
                                 new SqlDbType[] { SqlDbType.Int, SqlDbType.VarChar });
                    }

                }


                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

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
