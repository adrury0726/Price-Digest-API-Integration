using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This builder will be selecting in SizeClassID and ModelYear. This builder will be used in each of the Engines/Transmissions classes

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetMajorComponentsBuilder
    {
        private IDBConnection dBConnection;

        public GetMajorComponentsBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public MajorComponentsRequest Build(int subscriberID, int applicationNum)
        {
            MajorComponentsRequest size = new MajorComponentsRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "sub2007.sPriceDigestGetSizeClassID", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            size.SizeClassID = (int)PDUtility.nullToNullInt(dt.Rows[0]["SizeClassID"]);
            size.ModelYear = PDUtility.nullToNullString(dt.Rows[0]["ModelYear"]);
            return size;
        }
    }

}
