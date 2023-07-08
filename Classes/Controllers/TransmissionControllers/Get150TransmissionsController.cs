﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//Due to how the API Calls are built in Price Digest, I can only grab 50 Transmissions at a time. I'm going to have to create seperate classes in increments of 50 up to 250 to get all the data.

namespace PriceDigestAPI
{
    [Route("Post150Transmissions")]
    [ApiController]
    public class Get150TransmissionsController : ControllerBase
    {
        [HttpPost(Name = "Post150Transmissions")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {
                VehicleOptions ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                userNum = 0;
                string callType = "Get150TransmissionsController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //Using this to grab the SizeClassID and ModelYear for out call for the list of transmissions.
                MajorComponentsRequest components = new GetMajorComponentsBuilder(dBConnection).Build(subscriberId, applicationNum);

                // Check if SizeClassID is null
                if (components.SizeClassID == null)
                {
                    throw new Exception("SizeClassID is null");
                }

                // Check if SizeClassID is null
                if (components.ModelYear == null)
                {
                    throw new Exception("ModelYear is null");
                }

                //Endpoint we're using. THe offset 150 means it'll skip the first 150 results and give you the rest. The returns return only up to 150 results.
                string endpoint = "values/options?sizeClassId=" + components.SizeClassID + "&year=" + components.ModelYear + "&optionFamily=Transmission&offset=150";


                //actually make the get (call the price digest api)

                ret = apiCalls.MakeGet<VehicleOptions>(callId, subscriberId, endpoint, typeof(VehicleOptions));


                if (ret != null)
                {
                    foreach (OptionDetails con in ret.Options)
                    {
                        dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestTransmissionsAPICall",

                            //Model Properties
                            new object[] { components.ModelYear, components.SizeClassID, con.OptionFamilyID, con.OptionFamilyName, con.OptionName, con.OptionValue, con.OptionMSRP },

                            //Sproc variables
                            new string[] { "ModelYear", "SizeClassID", "OptionFamilyID", "OptionFamilyName", "OptionName", "OptionValue", "OptionMSRP" },

                            //Datatypes
                            new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar });
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
                return BadRequest("ERROR occurred. Check that your data is correct in the form. Most likely, ModelYear/SizeClassID one or both of those are null."); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
