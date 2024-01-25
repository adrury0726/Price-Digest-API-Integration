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
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            Valuation ret = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int assetNum = 0;
            int userNum = appBody.userNum;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //hard coded for now. you'll want to figure out how to pass in vins
            SpecsRequest configID = new IncompleteTruckValuationBuilder(dBConnection).Build(subscriberId, applicationNum);



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
                                                  SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int});;
               }


            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            return new OkResult();
        }
    }
}
