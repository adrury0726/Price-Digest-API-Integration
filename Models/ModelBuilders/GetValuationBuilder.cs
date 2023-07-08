using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This builder is being used in order to get the mileage

namespace PriceDigestAPI
{
    public class GetValuationBuilder
    {
        private IDBConnection dBConnection;

        public GetValuationBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public GetValuationMileage Build(int subscriberID, int applicationNum)
        {
            GetValuationMileage val = new GetValuationMileage();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestGetMajorComponents", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            val.Mileage = PDUtility.nullToNullString(dt.Rows[0]["Mileage"]);
            return val;
        }
    }

}
