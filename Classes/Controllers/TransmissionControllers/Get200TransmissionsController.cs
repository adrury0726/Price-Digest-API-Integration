using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//Due to how the API Calls are built in Price Digest, I can only grab 50 Transmissions at a time. I'm going to have to create seperate classes in increments of 50 up to 250 to get all the data.

namespace PriceDigestAPI
{
    [Route("Post200Transmissions")]
    [ApiController]
    public class Get200TransmissionsController : ControllerBase
    {
        [HttpPost(Name = "Post200Transmissions")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            VehicleOptions ret = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int userNum = 0;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //Reusing this for the configurationID
            MajorComponentsRequest components = new GetMajorComponentsBuilder(dBConnection).Build(subscriberId, applicationNum);


            //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using. This one contains the Engines. 
            string endpoint = "values/options?sizeClassId=" + components.SizeClassID + "&year=" + components.ModelYear + "&optionFamily=Transmission&offset=200";


            //actually make the get (call the price digest api)

            ret = apiCalls.MakeGet<VehicleOptions>(callId, subscriberId, endpoint, typeof(VehicleOptions));


            if (ret != null)
            {
                foreach (OptionDetails con in ret.Options)
                {

                    dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestTransmissionsAPICall",
                        new object[] { components.ModelYear, components.SizeClassID, con.OptionFamilyID, con.OptionFamilyName, con.OptionName, con.OptionValue, con.OptionMSRP },

                        new string[] { "ModelYear", "SizeClassID", "OptionFamilyID", "OptionFamilyName", "OptionName", "OptionValue", "OptionMSRP" },

                     new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar });

                }
            }

            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            //return the list if you want to (probably not going to be how info is returned to vision. My guess is you'll always update a table which vision will pull from. That means all your calls are probably going to be puts and posts)
            return new OkResult();
        }
    }
}
