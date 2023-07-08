using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This is used in order to grab the configurationID for the Incomplete Truck. It will be used in GetIncompleteTruckValuationController

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class IncompleteTruckValuationBuilder
    {
        private IDBConnection dBConnection;

        public IncompleteTruckValuationBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public SpecsRequest Build(int subscriberID, int applicationNum)
        {
            SpecsRequest spec = new SpecsRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestIncompleteTruckConfigID", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            spec.ConfigurationID = (int)PDUtility.nullToNullInt(dt.Rows[0]["ConfigurationID"]);
            return spec;
        }
    }

}
