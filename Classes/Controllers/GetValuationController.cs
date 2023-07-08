using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

// This class is grabbing the initial values for the asset that is selected

namespace PriceDigestAPI
{
    [Route("PostValuation")]
    [ApiController]
    public class GetValuationController : ControllerBase
    {
        [HttpPost(Name = "PostValuation")]
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
                string callType = "GetValuationController Class"; //Used to let devs know exactly where errors are occurring.

                //this is for logging. It logs that the attempt at a call has begun
                callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType);

                //This builder is being used to pass our ConfigurationID for us
                SpecsRequest specsRequest = new GetSpecsBuilder(dBConnection).Build(subscriberId, applicationNum);

                //Endpoint we're using for this call.
                string endpoint = "values/value?configurationId=" + specsRequest.ConfigurationID + "&condition=Good";


                //actually make the get (call the price digest api)
                ret = apiCalls.MakeGet<Valuation>(callId, subscriberId, endpoint, typeof(Valuation));


                if (ret != null)
                {
                    Valuation con = ret;

                    //Declaring these variables in order to handle when there's nulls
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
                    dBConnection.ExecSproc("Sub" + subscriberId + ".sPriceDigestValuationAPICall",

                                    //Model properties
                                    new object[] { applicationNum, subscriberId, assetNum, specsRequest.ConfigurationID, con.ModelId, con.ManufacturerId, con.ManufacturerName, con.ModelYear,
                                               con.MSRP, unadjustedFinanceParameter, unadjustedRetailParameter, unadjustedWholeSaleParameter, unadjustedTradeInParameter},

                                    //Sproc Variables
                                    new string[] { "ApplicationNum", "SubscriberID", "AssetNum", "ConfigurationID", "ModelId", "ManufacturerId", "ManufacturerName", "ModelYear",
                                               "MSRP", "UnadjustedFinance", "UnadjustedRetail", "UnadjustedWholesale", "UnadjustedTradeIn"},

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
