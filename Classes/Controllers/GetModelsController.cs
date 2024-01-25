using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;
using System.Collections.Generic;

namespace PriceDigestAPI
{
    [Route("PostModels")]
    [ApiController]
    public class GetModelsController : ControllerBase
    {
        [HttpPost(Name = "PostModels")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            // Hardcoded values for now
            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int userNum = 0;
            string callType = "this indicates the action being performed";

            // Start call logging
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            // Get the list of ClassificationIDs
            List<int> classificationIDs = new GetManufacturersBuilder(dBConnection).Build(subscriberId, applicationNum);

            if (classificationIDs != null && classificationIDs.Count > 0)
            {
                foreach (int classificationID in classificationIDs)
                {
                    string endpoint = $"bulk/models?classificationId={classificationID}";

                    // Make the API call
                    List<AllModels> models = apiCalls.MakeGetListObject<AllModels>(callId, subscriberId, endpoint, typeof(AllModels));

                    if (models != null && models.Count > 0)
                    {
                        foreach (AllModels model in models)
                        {
                            string modelAliases = model.ModelAliases != null ? string.Join(",", model.ModelAliases) : string.Empty;
                            string manufacturerAliases = model.ManufacturerAliases != null ? string.Join(",", model.ManufacturerAliases) : string.Empty;

                            // Execute the stored procedure
                            dBConnection.ExecSproc($"Sub{subscriberId}.PriceDigestModelsAPICall",
                                new object[] {classificationID, model.ModelID, model.ModelName, modelAliases, model.ManufacturerID, model.ManufacturerName, manufacturerAliases },

                                new string[] { "classificationID", "ModelID", "ModelName", "ModelAliases", "ManufacturerID", "ManufacturerName", "ManufacturerAliases" },

                                new SqlDbType[] {SqlDbType.Int ,SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar });
                        }
                    }
                }
            }

            // End call logging
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            return new OkResult();
        }
    }
}
