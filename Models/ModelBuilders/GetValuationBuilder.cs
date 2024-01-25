using PriceDigestAPI.Models.GetRequests;
using System.Data;

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
            dBConnection.ExecSproc(ref dt, "Sub" + subscriberID + ".sPriceDigestGetMajorComponents", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            val.Mileage = PDUtility.nullToNullString(dt.Rows[0]["Mileage"]);
            return val;
        }
    }

}
