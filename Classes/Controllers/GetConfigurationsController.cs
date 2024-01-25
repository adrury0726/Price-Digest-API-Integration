using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;


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
            List<Configurations> ret = null;
            List<DisplaySpec> displaySpecs = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int assetNum = 0;
            int userNum = appBody.userNum;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //ConfigRequest configRequest = new GetConfigurationsBuilder(dBConnection).Build(subscriberId, applicationNum);
            List<int> configurationIDs = new GetConfigurationsBuilder(dBConnection).Build(subscriberId, applicationNum);

            //We need it to loop through all of the configurationID's that are in the table for that appnum.
            if (configurationIDs != null && configurationIDs.Count > 0)
            {
                foreach (int configurationID in configurationIDs)
                {

                    //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using 
                    string endpoint = "details/configurations?configurationId=" + configurationID;
            //Can use https://pricedigestsapi.com/v1/details/configurations?manufacturer=Ford&model=F-150&year=2019

            //actually make the get (call the price digest api)
            ret = apiCalls.MakeGetListObject<Configurations>(callId, subscriberId, endpoint, typeof(Configurations));


                    if (ret != null)
                    {
                        foreach (Configurations con in ret)
                        {

                            dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestConfigurationsAPICall",
                                new object[] { applicationNum, subscriberId, assetNum, con.ConfigurationId, con.ModelId, con.ModelName, con.ModelYear, con.ManufacturerId, con.ManufacturerName, con.ClassificationId,
                                       con.ClassificationName, con.CategoryId, con.CategoryName, con.SubtypeId, con.SubtypeName, con.SizeClassId, con.SizeClassName, con.SizeClassMin, con.SizeClassMax,
                                       con.SizeClassUom, con.VinModelNumber},

                                new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationID", "ModelID", "ModelName", "ModelYear", "ManufacturerID", "ManufacturerName", "ClassificationID",
                                       "ClassificationName", "CategoryID", "CategoryName", "SubtypeId", "SubtypeName", "SizeClassID", "SizeClassName", "SizeClassMin", "SizeClassMax",
                                       "SizeClassUom", "VinModelNumber"},

                             new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int,
                                       SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int,
                                       SqlDbType.VarChar, SqlDbType.VarChar});

                            foreach (DisplaySpec spec in con.DisplaySpecs)
                            {

                                dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestDisplaySpecsAPICall",
                                    new object[] { applicationNum, subscriberId, assetNum, spec.SpecName, spec.SpecValue, spec.SpecNameFriendly, spec.SpecUom, spec.SpecDescription, spec.SpecFamily, con.ConfigurationId },

                                    new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "SpecName", "SpecValue", "SpecNameFriendly", "SpecUom", "SpecDescription", "SpecFamily", "ConfigurationId" },

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

    }
}