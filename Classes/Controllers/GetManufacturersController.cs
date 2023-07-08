using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;
using System.Collections.Generic;

//This class is being built in order to retrieve all of the manufacturers for the VehicleMake drop-down

namespace PriceDigestAPI
{
    [Route("PostManufacturers")]
    [ApiController]
    public class GetManufacturersController : ControllerBase
    {
        [HttpPost(Name = "PostManufacturers")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {
                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                userNum = 0;
                string callType = "GetManufacturersController Class"; //Used to let devs know exactly where errors are occurring.

                // Start call logging
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                // Loop through our ClassificationIDs
                List<int> classificationIDs = new GetManufacturersBuilder(dBConnection).Build(subscriberId, applicationNum);

                //Plug in the classificationID for each value that's available. 
                if (classificationIDs != null && classificationIDs.Count > 0)
                {
                    foreach (int classificationID in classificationIDs)
                    {
                        //The endpoint we're using
                        string endpoint = $"bulk/manufacturers?classificationId={classificationID}";

                        // Make the API call. Looping it through each ClassificationID and getting that info.
                        List<Manufacturers> manufacturers = apiCalls.MakeGetListObject<Manufacturers>(callId, subscriberId, endpoint, typeof(Manufacturers));


                        if (manufacturers != null && manufacturers.Count > 0)
                        {
                            foreach (Manufacturers manufacturer in manufacturers)
                            {
                                string manufacturerAliases = manufacturer.ManufacturerAliases != null ? string.Join(",", manufacturer.ManufacturerAliases) : string.Empty;

                                // Execute the stored procedure
                                dBConnection.ExecSproc($"Sub{subscriberId}.PriceDigestManufacturersAPICall",

                                    //Model properties
                                    new object[] { classificationID, manufacturer.ManufacturerID, manufacturer.ManufacturerName, manufacturerAliases },

                                    //Sproc Variables
                                    new string[] { "ClassificationID", "ManufacturerID", "ManufacturerName", "ManufacturerAliases" },

                                    //Datatypes
                                    new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar });
                            }
                        }
                    }
                }
                // End call logging
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                return new OkResult();
            }
            catch (Exception ex)
            {
                callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("Invalid input. Check that the data in the table for the classificationID"); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
