using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;
using System.Collections.Generic;

namespace PriceDigestAPI
{
    [Route("PostManufacturers")]
    [ApiController]
    public class GetManufacturersController : ControllerBase
    {
        [HttpPost(Name = "PostManufacturers")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            // Hardcoded values for now
            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int userNum = 0;
            string callType = "this indicates the action being performed";

            // Start call logging
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            // Loop through our ClassificationIDs
            List<int> classificationIDs = new GetManufacturersBuilder(dBConnection).Build(subscriberId, applicationNum);

            if (classificationIDs != null && classificationIDs.Count > 0)
            {
                foreach (int classificationID in classificationIDs)
                {
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
                                new object[] { classificationID, manufacturer.ManufacturerID, manufacturer.ManufacturerName, manufacturerAliases },
                                new string[] { "ClassificationID", "ManufacturerID", "ManufacturerName", "ManufacturerAliases" },
                                new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar });
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
