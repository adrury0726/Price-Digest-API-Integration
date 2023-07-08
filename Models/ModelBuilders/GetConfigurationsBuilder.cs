using System.Data;
using PriceDigestAPI.Models.GetRequests;

//This builder is being used in order to grab the ConfigurationID. This will be used in the GetConfigurationsController

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetConfigurationsBuilder
    {
        private IDBConnection dBConnection;

        public GetConfigurationsBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public List<int> Build(int subscriberID, int applicationNum)
        {
            List<int> configurationIDs = new List<int>();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestSendConfigIDs", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            foreach (DataRow row in dt.Rows)
            {
                int configurationID = (int)PDUtility.nullToNullInt(row["ConfigurationID"]);
                configurationIDs.Add(configurationID);
            }

            return configurationIDs;
        }

    }
}
