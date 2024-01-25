using System.Data;
using PriceDigestAPI.Models.GetRequests;

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
            dBConnection.ExecSproc(ref dt, "Sub" + subscriberID + ".sPriceDigestSendConfigIDs", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
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
