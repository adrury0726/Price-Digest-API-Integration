using System.Data;
using PriceDigestAPI.Models.GetRequests;

//This builder is used to get our Vin/Year/Make/Model that's in the first form of Price Digest. We'll be using this in the GetVerificationsController

namespace PriceDigestAPI
{
    public class BasicGetRequestBuilder
    {
        private IDBConnection dBConnection;
        
        public BasicGetRequestBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public BasicGetRequest Build(int subscriberID, int applicationNum) 
        { 
            BasicGetRequest modelMake = new BasicGetRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestGetVIN", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            modelMake.VinNum = PDUtility.nullToNullString(dt.Rows[0]["VinNum"]);
            modelMake.VehicleCategory = PDUtility.nullToNullString(dt.Rows[0]["VehicleCategory"]);
            modelMake.VehicleMake = PDUtility.nullToNullString(dt.Rows[0]["VehicleMake"]);
            modelMake.VehicleModel = PDUtility.nullToNullString(dt.Rows[0]["VehicleModel"]);
            modelMake.VehicleYear = PDUtility.nullToNullString(dt.Rows[0]["VehicleYear"]);
            return modelMake;
        }
    }
}
