using Microsoft.AspNetCore.Mvc;
using PriceDigestAPI.Classes;
using System.Data;

namespace PriceDigestAPI
{
    [Route("PostOptions")]
    [ApiController]
    public class GetOptionsController : ControllerBase
    {
        [HttpGet(Name = "PostOptions")]
        //as you can see, you have to pass in the apicalls, calllogging, and dbconnection classes. This will be necessary any time you need to use those clases in a controller (likely always) and the format is as below
        public IActionResult Post([FromServices] IAPICalls apiCalls, [FromServices] ILogging callLogging, [FromServices] IDBConnection dBConnection, AppBody appBody)
        {
            VehicleOptions ret = null;

            //these are hardcoded for now, need to eventually be passed in somehow
            //they're used for logging. SubscriberID is used elsewhere as well
            //logging isn't working right now, but you should still build around it
            //maybe your integration won't care about application num, I don't know under what context it's getting called. At a minimum it's going to care about subId and usernum for logging though
            //I've just left appnum in because the logging I already built requires it. Can be removed down the line
            int subscriberId = appBody.subscriberId;
            int applicationNum = appBody.applicationNum;
            int assetNum = 0;
            int userNum = appBody.userNum;
            string callType = "this indicates the action being performed";

            //this is for logging. It logs that the attempt at a call has begun
            long? callId = callLogging.startCall(subscriberId, applicationNum, userNum, callType, appBody.CompanyDB);

            //hard coded for now. you'll want to figure out how to pass in variables
            string sizeClassID = "502";
            string modelYear = "2013";

            //here's an example of how to execute a sproc. You pass in the name and the parameters as follows
            //right now this sproc just returns user info as an example of how to get info back from our db
     //    DataTable dt = new DataTable();
     //    dBConnection.ExecSproc(ref dt, "Sub" + subscriberId + ".PriceDigestExampleGetInfoSproc", new object[] { userNum }, new string[] { "UserNum" }, new SqlDbType[] { SqlDbType.Int });
     //
     //    //here's how you'd get info out of the results of a sproc
     //    string name = PDUtility.NullToString(dt.Rows[0]["UserFullName"]);
     //    string birthday = PDUtility.NullToString(dt.Rows[0]["Birthdate"]);
     //    if (birthday == "")
     //        birthday = "apparently never. Sorry about your nonexistence ig";
     //    Console.WriteLine("hi " + name + ", your birthday is " + birthday);


            //below is how you make the call once you have the info you need to do so. You don't need to pass the base url, just the specific endpoint and parameters you're using 
            string endpoint = "values/options/?sizeClassId=" + sizeClassID + "&year=" + modelYear;

            //actually make the get (call the price digest api)
            ret = apiCalls.MakeGet<VehicleOptions>(callId, subscriberId, endpoint, typeof(VehicleOptions));


            if (ret != null)
            {  
                    //this is an example of updating the database with the info you got from the api
                    //this example also includes 2 parameters so you can see how to pass multiple params to a sproc
                    //right now this sproc basically just logs whatever you put into it into a dummy table
                    //so we're using it to add every returned model id to that table
                    dBConnection.ExecSproc("Sub" + subscriberId + ".PriceDigestExampleSproc", new object[] { "ClassificationID", ret.ClassificationID }, new string[] { "FieldName", "FieldValue" }, new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar });
            }


            //again, logging. logs that the call has completed
            callLogging.endCall(subscriberId, applicationNum, userNum, callId);

            //return the list if you want to (probably not going to be how info is returned to vision. My guess is you'll always update a table which vision will pull from. That means all your calls are probably going to be puts and posts)
            return new OkResult();
        }
    }
}
