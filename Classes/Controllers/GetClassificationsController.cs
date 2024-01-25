using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This is where we're doing our Verifications call to the API. User can enter a VIN and it will grab all of the necessary information.
//Figure out if these should actually be gets, posts, puts

namespace PriceDigestAPI
{
    [Route("PostClassifications")]
    [ApiController]
    public class GetClassificationsController : ControllerBase
    {
        [HttpPost(Name = "PostClassifications")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            List<Classifications> ret = null;

            //these are hardcoded for now, need to eventually be passed in somehow
            //they're used for logging. SubscriberID is used elsewhere as well
            //logging isn't working right now, but you should still build around it
            //maybe your integration won't care about application num, I don't know under what context it's getting called. At a minimum it's going to care about subId and usernum for logging though
            //I've just left appnum in because the logging I already built requires it. Can be removed down the line
            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int userNum = 0;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);


            //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using 
            string endpoint = "";


            endpoint += "taxonomy/classifications";

            //actually make the get (call the price digest api)
            ret = apiCalls.MakeGetListObject<Classifications>(callId, subscriberId, endpoint, typeof(Classifications));


               if (ret != null)
              {
                   foreach (Classifications con in ret)
                   {

                      //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                       //Model properties
                       dBConnection.ExecSproc("Sub" + subscriberId + ".PriceDigestClassificationsAPICall",
                                new object[] {con.ClassificationID, con.ClassificationName},

                                //Sproc Variables
                                new string[] {"ClassificationID", "ClassificationName"},
                                
                                //You guessed it, data types
                                new SqlDbType[] { SqlDbType.Int, SqlDbType.VarChar});
                   }

               }


            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            return new OkResult();
        }
    }
}
