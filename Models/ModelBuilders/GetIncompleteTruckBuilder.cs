using PriceDigestAPI.Models.GetRequests;
using System.Data;

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetIncompleteTruckBuilder
    {
        private IDBConnection dBConnection;

        public GetIncompleteTruckBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public IncompleteTruckRequest Build(int subscriberID, int applicationNum)
        {
            IncompleteTruckRequest truck = new IncompleteTruckRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "Sub" + subscriberID + ".sPriceDigestGetTruckBodyBuild", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            truck.Category = PDUtility.nullToNullString(dt.Rows[0]["Category"]);
            truck.ModelName = PDUtility.nullToNullString(dt.Rows[0]["ModelName"]);
            truck.ModelYear = PDUtility.nullToNullString(dt.Rows[0]["ModelYear"]);

            return truck;
        }
    }

}
