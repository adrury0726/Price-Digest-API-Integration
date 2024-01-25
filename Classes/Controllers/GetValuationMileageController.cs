using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PriceDigestAPI.Classes;
using PriceDigestAPI.Models;
using PriceDigestAPI.Models.GetRequests;
using PriceDigestAPI.Models.ModelBuilders;
using System.Data;

// This class is doing the same thing as the GetValuationController. HOwever, this one will actually be used in Vision when Mileage may be present. 
// It needed to be a seperate class, as we'll be calling the endpoint at a different time.


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
            Valuation ret = null;

            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int assetNum = 0;
            int userNum = appBody.userNum;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //hard coded for now. you'll want to figure out how to pass in vins
            SpecsRequest specsRequest = new GetSpecsBuilder(dBConnection).Build(subscriberId, applicationNum);
            GetValuationMileage valuation = new GetValuationBuilder(dBConnection).Build(subscriberId, applicationNum);



            //string endpoint = "values/value?configurationId=" + specsRequest.ConfigurationID + "&condition=Good&usage=" + valuation.Mileage;
            //We need this in order to get the correct values if there is no mileage
            string endpoint = "values/value?configurationId=" + specsRequest.ConfigurationID + "&condition=Good";
            if (valuation.Mileage != null)
            {
                endpoint += "&usage=" + valuation.Mileage;
            }


            //actually make the get (call the price digest api)
            ret = apiCalls.MakeGet<Valuation>(callId, subscriberId, endpoint, typeof(Valuation));


               if (ret != null)
              {
                Valuation con = ret;

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
                                                  SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int});;
               }


            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            return new OkResult();
        }
    }
}
