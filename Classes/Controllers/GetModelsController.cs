using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;
using System.Collections.Generic;

//This class is being used to pull in all of the available models in Price Digest for the VehicleModel drop-down

namespace PriceDigestAPI
{
    [Route("PostModels")]
    [ApiController]
    public class GetModelsController : ControllerBase
    {
        [HttpPost(Name = "PostModels")]
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
                string callType = "GetModelsController Class"; //Used to let devs know exactly where errors are occurring.

                // Start call logging
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                // Get the list of ClassificationIDs from this builder
                List<int> classificationIDs = new GetManufacturersBuilder(dBConnection).Build(subscriberId, applicationNum);

                //Plug in the classificationID for each value that's available. 
                if (classificationIDs != null && classificationIDs.Count > 0)
                {
                    foreach (int classificationID in classificationIDs)
                    {
                        string endpoint = $"bulk/models?classificationId={classificationID}";

                        // Make the API call
                        List<AllModels> models = apiCalls.MakeGetListObject<AllModels>(callId, subscriberId, endpoint, typeof(AllModels));

                        //Plugging in the values for wherever the models are available
                        if (models != null && models.Count > 0)
                        {
                            foreach (AllModels model in models)
                            {
                                string modelAliases = model.ModelAliases != null ? string.Join(",", model.ModelAliases) : string.Empty;
                                string manufacturerAliases = model.ManufacturerAliases != null ? string.Join(",", model.ManufacturerAliases) : string.Empty;

                                // Execute the stored procedure
                                dBConnection.ExecSproc($"Sub{subscriberId}.PriceDigestModelsAPICall",

                                    //Model properties
                                    new object[] { classificationID, model.ModelID, model.ModelName, modelAliases, model.ManufacturerID, model.ManufacturerName, manufacturerAliases },

                                    //Stored Procedure Variables
                                    new string[] { "classificationID", "ModelID", "ModelName", "ModelAliases", "ManufacturerID", "ManufacturerName", "ManufacturerAliases" },

                                    //Datatypes
                                    new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar });
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
                return BadRequest("Invalid input. Check that the data is the table for the ClassificationID"); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
