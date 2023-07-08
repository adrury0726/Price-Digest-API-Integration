using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

// This call is being used to get the valuation of an asset when it has mileage, as it changes the pricing the asset. This will be used in GetValuationMileageController


namespace PriceDigestAPI
{
    [Route("PostValuationWithMileage")]
    [ApiController]
    public class GetValuationMileageController : ControllerBase
    {
        [HttpPost(Name = "PostValuationWithMileage")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
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
                int assetNum = 0;
                userNum = appBody.userNum;
                string callType = "PostValuationWithMileage Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //Using this builder in order to grab the ConfigurationID to plug in to the call.
                SpecsRequest specsRequest = new GetSpecsBuilder(dBConnection).Build(subscriberId, applicationNum);

                //Using this builder in order to get the Mileage if there is any to plug in to the call
                GetValuationMileage valuation = new GetValuationBuilder(dBConnection).Build(subscriberId, applicationNum);


                //We need this in order to get the correct values if there is mileage or not
                string endpoint = "values/value?configurationId=" + specsRequest.ConfigurationID + "&condition=Good";
                if (valuation.Mileage != null)
                {
                    endpoint += "&usage=" + valuation.Mileage;
                }

                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGet<Valuation>(callId, subscriberId, endpoint, typeof(Valuation));

                if (ret != null)
                {
                    //Model we're pulling our variables from
                    Valuation con = ret;

                    //Declare variables so they can be handled if they're null
                    int? adjustedFinance = con.AdjustedFinance;
                    int? adjustedRetail = con.AdjustedRetail;
                    int? adjustedWholeSale = con.AdjustedWholesale;
                    int? adjustedTradeIn = con.AdjustedTradeIn;

                    //Handle the variable if it is null here.
                    object adjustedFinanceParameter = adjustedFinance.HasValue ? (object)adjustedFinance.Value : DBNull.Value;
                    object adjustedRetailParameter = adjustedRetail.HasValue ? (object)adjustedRetail.Value : DBNull.Value;
                    object adjustedWholeSaleParameter = adjustedWholeSale.HasValue ? (object)adjustedWholeSale.Value : DBNull.Value;
                    object adjustedTradeInParameter = adjustedTradeIn.HasValue ? (object)adjustedTradeIn.Value : DBNull.Value;


                    //This code is where we're taking what's returned from the API Get Request and running the save sproc that stores all this info into their respective tables
                    dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestValuationAdjustmentsAPICall",

                                    //Model properties
                                    new object[] { applicationNum, subscriberId, assetNum, specsRequest.ConfigurationID, con.ModelId, con.ManufacturerId, con.ManufacturerName, con.ModelYear,
                                               con.MSRP, adjustedFinanceParameter, adjustedWholeSaleParameter, adjustedRetailParameter, adjustedTradeInParameter},

                                    //Sproc Variables
                                    new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationID", "ModelId", "ManufacturerId", "ManufacturerName", "ModelYear",
                                               "MSRP", "AdjustedFinance", "AdjustedWholesale", "AdjustedRetail", "AdjustedTradeIn"},

                                    //You guessed it, data types
                                    new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar,
                                                  SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int}); ;
                }


                //again, logging. logs that the call has completed
                callLogging.endCall(subscriberId, applicationNum, userNum, callId);

                return new OkResult();
            }
            catch (Exception ex)
            {
                callLogging.logError(subscriberId, applicationNum, userNum, callId, ex);
                return BadRequest("Invalid input. Check that the data is correct in the form."); // Returns a 400 Bad Request status code with a custom error message
            }
        }
    }
}
