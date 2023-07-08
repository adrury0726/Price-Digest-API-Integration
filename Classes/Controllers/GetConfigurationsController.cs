using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//This class is being created in order to get the list of configurations that are available once an option has been chosen in the ConfigOptions drop-down

namespace PriceDigestAPI
{
    [Route("PostConfigurations")]
    [ApiController]
    public class GetConfigurationsController : ControllerBase
    {
        [HttpPost(Name = "PostConfigurations")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {

                List<Configurations> ret = null;
                List<DisplaySpec> displaySpecs = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                int assetNum = 0;
                userNum = appBody.userNum;
                string callType = "PostConfigurations Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //This builder is grabbing all of the configurationIDs from the table. The list is based on what's saved in the ConfigOptions drop-down
                List<int> configurationIDs = new GetConfigurationsBuilder(dBConnection).Build(subscriberId, applicationNum);

                // Check if configID is null
                if (configurationIDs == null || configurationIDs.Count < 1)
                {
                    throw new Exception("configurationIDs is null");
                }

                //We need it to loop through all of the configurationID's that are in the table for that appnum.
                if (configurationIDs != null && configurationIDs.Count > 0)
                {
                    foreach (int configurationID in configurationIDs)
                    {

                        //Endpoint we'll be using for our call 
                        string endpoint = "details/configurations?configurationId=" + configurationID;

                        //actually make the get (call the price digest api)
                        ret = apiCalls.MakeGetListObject<Configurations>(callId, subscriberId, endpoint, typeof(Configurations));


                        if (ret != null)
                        {
                            foreach (Configurations con in ret)
                            {

                                dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestConfigurationsAPICall",

                                    //Model properties
                                    new object[] { applicationNum, subscriberId, assetNum, con.ConfigurationId, con.ModelId, con.ModelName, con.ModelYear, con.ManufacturerId, con.ManufacturerName, con.ClassificationId,
                                       con.ClassificationName, con.CategoryId, con.CategoryName, con.SubtypeId, con.SubtypeName, con.SizeClassId, con.SizeClassName, con.SizeClassMin, con.SizeClassMax,
                                       con.SizeClassUom, con.VinModelNumber},

                                    //Sproc Variables
                                    new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationID", "ModelID", "ModelName", "ModelYear", "ManufacturerID", "ManufacturerName", "ClassificationID",
                                       "ClassificationName", "CategoryID", "CategoryName", "SubtypeId", "SubtypeName", "SizeClassID", "SizeClassName", "SizeClassMin", "SizeClassMax",
                                       "SizeClassUom", "VinModelNumber"},

                                    //Datatypes
                                    new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int,
                                       SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int,
                                       SqlDbType.VarChar, SqlDbType.VarChar});

                                foreach (DisplaySpec spec in con.DisplaySpecs)
                                {

                                    dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestDisplaySpecsAPICall",

                                        //Model properties
                                        new object[] { applicationNum, subscriberId, assetNum, spec.SpecName, spec.SpecValue, spec.SpecNameFriendly, spec.SpecUom, spec.SpecDescription, spec.SpecFamily, con.ConfigurationId },

                                        //Sproc Variables
                                        new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "SpecName", "SpecValue", "SpecNameFriendly", "SpecUom", "SpecDescription", "SpecFamily", "ConfigurationId" },

                                        //Datatypes
                                        new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int });
                                }
                            }
                        }
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
                return BadRequest("ConfigurationIDs are null or do not exist. Check that the data is correct in the form/builder."); // Returns a 400 Bad Request status code with a custom error message (check table for configurationIDs)
            }
        }
    }
}