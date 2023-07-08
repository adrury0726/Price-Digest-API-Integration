using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//This is where we're doing our Verifications call to the API. User can enter a VIN or Year/Make/Model and it will grab all of the necessary information.

namespace PriceDigestAPI
{
    [Route("PostVerifications")]
    [ApiController]
    public class GetVerificationsController : ControllerBase
    {
        [HttpPost(Name = "PostVerifications")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {

            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1; 
            long? callId = -1;

            try
            {

                List<Verification> ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                userNum = appBody.userNum;
                string callType = "GetVerificationsController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //Using our builder to get the VinNum or Year/Make/Model for API call
                BasicGetRequest basicGetRequest = new BasicGetRequestBuilder(dBConnection).Build(subscriberId, applicationNum);

                //Using this builder to group up each value returned to all be assigned the same assetNum
                GetAssetNumRequest assetNumRequest = new GetAssetNumBuilder(dBConnection).Build(subscriberId, applicationNum);

                int assetNum = assetNumRequest.AssetNum;

                // Check if the assetNum is null or 0. We want each call to assign the assetNum a value so they're grouped as 1 call.
                if (assetNum == null || assetNum == 0)
                {
                    assetNum = 1;
                }
                else
                {
                    assetNum++; //Increment the call by 1 based on whatever is returned by the builder. IE, assetNumRequest.AssetNum + 1
                }

                //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using 
                string endpoint = "";


                //Whether we have a VIN # or not will dictate which API call we use.
                if (basicGetRequest.VinNum != null)
                {
                    endpoint += "verification/vin/" + basicGetRequest.VinNum;
                }
                else
                {
                    endpoint += "taxonomy/configurations/?manufacturer=" + basicGetRequest.VehicleMake + "&model=" + basicGetRequest.VehicleModel + "&year=" + basicGetRequest.VehicleYear;
                }

                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGetListObject<Verification>(callId, subscriberId, endpoint, typeof(Verification));



                if (ret != null)
                {
                    foreach (Verification con in ret)
                    {
                        //These variables are lists, so we need to save them differently. If they aren't null, it returns the data. Otherwise, empty string.
                        string manufacturerAliases = con.ManufacturerAliases != null ? string.Join(",", con.ManufacturerAliases) : string.Empty;
                        string modelAliases = con.ModelAliases != null ? string.Join(",", con.ModelAliases) : string.Empty;

                        //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                        dBConnection.ExecSproc("Sub" + subscriberId + ".PriceDigestVerificationAPIRequestSave",

                                 //Model properties
                                 new object[] { applicationNum, subscriberId, assetNum, con.ModelID, con.ModelName, con.ModelYear, con.ManufacturerID, con.ManufacturerName, con.ClassificationID, con.ClassificationName,
                                               con.CategoryID, con.CategoryName, con.SubTypeID, con.SubTypeName, con.SizeClassID, con.SizeClassName, con.SizeClassMin, con.SizeClassMax, con.SizeClassUom,
                                               con.ConfigurationID, con.VinModelNumber, con.VinManufacturerCode, con.VinYearCode, manufacturerAliases, modelAliases, con.CicCode, con.ShortVin, con.Brand},

                                 //Sproc Variables
                                 new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ModelID", "ModelName", "ModelYear", "ManufacturerID", "ManufacturerName", "ClassificationID", "ClassificationName",
                                                "CategoryID", "CategoryName", "SubTypeID", "SubTypeName", "SizeClassID", "SizeClassName", "SizeClassMin", "SizeClassMax", "SizeClassUom",
                                               "ConfigurationID", "VinModelNumber", "VinManufacturerCode", "VinYearCode", "ManufacturerAlias", "ModelAlias", "CicCode", "ShortVin", "Brand"},

                                 //You guessed it, datatypes
                                 new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar,
                                                  SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar,
                                                  SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar});
                    }

                }


                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                return new OkResult();
            }
            catch (Exception ex)
            {
               callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("Invalid input. Check that the VIN # or YMM are correct in the form."); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
