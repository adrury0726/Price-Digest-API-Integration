using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

// This class is grabbing the configurationID for the selected incomplete truck body and inserting the prices for the asset into its appropriate table by running the Sproc Sub2007.sPriceDigestValuationAPICall

namespace PriceDigestAPI
{
    [Route("PostIncompleteTruckValuation")]
    [ApiController]
    public class GetIncompleteTruckValuationController : ControllerBase
    {
        [HttpPost(Name = "PostIncompleteTruckValuation")]
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            int subscriberId = 2007;
            int applicationNum = -1; //Want default values because if the appbody is wrong, it won't reach the catch block. Need to know if it's not hitting that point.
            int userNum = -1;
            long? callId = -1;

            try
            {
                Valuation ret = null;

                subscriberId = appBody.subscriberId;
                applicationNum = appBody.applicationNum;
                userNum = appBody.userNum;
                string callType = "PostIncompleteTruckValuation Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //We're using this builder to grab the ConfigurationID for us to plug in to the API call
                SpecsRequest configID = new IncompleteTruckValuationBuilder(dBConnection).Build(subscriberId, applicationNum);

                // Check if configID is null
                if (configID == null)
                {
                    throw new Exception("configID is null");
                }

                //Endpoint we're using for the API call
                string endpoint = "values/value?configurationId=" + configID.ConfigurationID + "&condition=Good";


                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGet<Valuation>(callId, subscriberId, endpoint, typeof(Valuation));


                if (ret != null)
                {
                    Valuation con = ret;

                    //Added this for when there's a null value. Was getting errors before adding in this code.
                    int? unadjustedFinance = con.UnadjustedFinance;
                    int? unadjustedRetail = con.UnadjustedRetail;
                    int? unadjustedWholeSale = con.UnadjustedWholesale;
                    int? unadjustedTradeIn = con.UnadjustedTradeIn;

                    //Handling the null value here
                    object unadjustedFinanceParameter = unadjustedFinance.HasValue ? (object)unadjustedFinance.Value : DBNull.Value;
                    object unadjustedRetailParameter = unadjustedRetail.HasValue ? (object)unadjustedRetail.Value : DBNull.Value;
                    object unadjustedWholeSaleParameter = unadjustedWholeSale.HasValue ? (object)unadjustedWholeSale.Value : DBNull.Value;
                    object unadjustedTradeInParameter = unadjustedTradeIn.HasValue ? (object)unadjustedTradeIn.Value : DBNull.Value;

                    //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                    //Model properties
                    dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestIncompleteTruckValuationAPICall",

                                    //Model properties
                                    new object[] { applicationNum, subscriberId, configID.ConfigurationID, con.ModelId, con.ModelName, con.ManufacturerId, con.ManufacturerName, con.ClassificationId,
                                               con.ClassificationName, con.CategoryId, con.SizeClassId, con.SizeClassName, con.ModelYear,
                                               con.MSRP, unadjustedFinanceParameter, unadjustedRetailParameter, unadjustedWholeSaleParameter, unadjustedTradeInParameter},

                                    //Sproc Variables
                                    new string[] { "ApplicationNum", "SubscriberID", "ConfigurationID", "ModelId", "ModelName", "ManufacturerId", "ManufacturerName", "ClassificationId" ,
                                               "ClassificationName", "CategoryId", "SizeClassId", "SizeClassName", "ModelYear",
                                               "MSRP", "UnadjustedFinance", "UnadjustedRetail", "UnadjustedWholesale", "UnadjustedTradeIn"},

                                    //You guessed it, data types
                                    new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int,
                                                  SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar,
                                                  SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int}); ;
                }
                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                return new OkResult();
            }
            catch (Exception ex)
            {
                callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("ConfigID not present. Check that the data is correct in the form."); // Check the builder so you can see if there's a configID being pulled. 
            }
        }
    }
}
