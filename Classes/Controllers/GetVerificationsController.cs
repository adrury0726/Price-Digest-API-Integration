using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

//This is where we're doing our Verifications call to the API. User can enter a VIN and it will grab all of the necessary information.
//Figure out if these should actually be gets, posts, puts

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
            List<Verification> ret = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int userNum = appBody.userNum;
            string callType = "Verification EndPoint";



            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //hard coded for now. you'll want to figure out how to pass in vins
            BasicGetRequest basicGetRequest = new BasicGetRequestBuilder(dBConnection).Build(subscriberId, applicationNum);
            GetAssetNumRequest assetNumRequest = new GetAssetNumBuilder(dBConnection).Build(subscriberId, applicationNum);

            int assetNum = assetNumRequest.AssetNum;

            // Check if the assetNum is null or 0. We want each call to assign the assetNum a value so they're grouped as 1 call.
            if (assetNum == null || assetNum == 0)
            {
                assetNum = 1;
            }
            else
            {
                assetNum++;
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
                       //Model properties
                       dBConnection.ExecSproc("Sub" + subscriberId + ".PriceDigestVerificationAPIRequestSave",
                                new object[] { applicationNum, subscriberId, assetNum, con.ModelID, con.ModelName, con.ModelYear, con.ManufacturerID, con.ManufacturerName, con.ClassificationID, con.ClassificationName, 
                                               con.CategoryID, con.CategoryName, con.SubTypeID, con.SubTypeName, con.SizeClassID, con.SizeClassName, con.SizeClassMin, con.SizeClassMax, con.SizeClassUom,
                                               con.ConfigurationID, con.VinModelNumber, con.VinManufacturerCode, con.VinYearCode, manufacturerAliases, modelAliases, con.CicCode, con.ShortVin, con.Brand},

                                //Sproc Variables
                                new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ModelID", "ModelName", "ModelYear", "ManufacturerID", "ManufacturerName", "ClassificationID", "ClassificationName", 
                                                "CategoryID", "CategoryName", "SubTypeID", "SubTypeName", "SizeClassID", "SizeClassName", "SizeClassMin", "SizeClassMax", "SizeClassUom",
                                               "ConfigurationID", "VinModelNumber", "VinManufacturerCode", "VinYearCode", "ManufacturerAlias", "ModelAlias", "CicCode", "ShortVin", "Brand"},
                                
                                //You guessed it, data types
                                new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar,
                                                  SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, 
                                                  SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar});
                   }

               }


            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            return new OkResult();
        }
    }
}
