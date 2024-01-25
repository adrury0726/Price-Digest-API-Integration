using System.Data;
using PriceDigestAPI.Models.GetRequests;

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetManufacturersBuilder
    {
        private IDBConnection dBConnection;

        public GetManufacturersBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public List<int> Build(int subscriberID, int applicationNum)
        {
            List<int> classificationIDs = new List<int>();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "Sub" + subscriberID + ".sPriceDigestSelectingClassificationIDs", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            foreach (DataRow row in dt.Rows)
            {
                int classificationID = (int)PDUtility.nullToNullInt(row["ClassificationID"]);
                classificationIDs.Add(classificationID);
            }

            return classificationIDs;
        }

    }
}
