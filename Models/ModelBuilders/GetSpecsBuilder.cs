using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This is being used in order to retrieve the ConfigurationID. This will be used in the GetValuationController and the GetSpecificationsController

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetSpecsBuilder
    {
        private IDBConnection dBConnection;

        public GetSpecsBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public SpecsRequest Build(int subscriberID, int applicationNum)
        {
            SpecsRequest spec = new SpecsRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestValuationConfigID", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            spec.ConfigurationID = (int)PDUtility.nullToNullInt(dt.Rows[0]["ConfigurationID"]);
            return spec;
        }
    }

}
