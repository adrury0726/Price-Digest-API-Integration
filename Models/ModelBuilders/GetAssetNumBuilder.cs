using PriceDigestAPI.Models.GetRequests;
using System.Data;

//This builder is being used in order to retrieve the AssetNum of an asset. We'll be using this in the GetVerificationsController

namespace PriceDigestAPI.Models.ModelBuilders
{
    public class GetAssetNumBuilder
    {
        private IDBConnection dBConnection;

        public GetAssetNumBuilder(IDBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        public GetAssetNumRequest Build(int subscriberID, int applicationNum)
        {
            GetAssetNumRequest asset = new GetAssetNumRequest();

            DataTable dt = new DataTable();
            dBConnection.ExecSproc(ref dt, "Sub2007.PriceDigestGetAssetNum", new Object[] { subscriberID, applicationNum }, new string[] { "SubscriberID", "ApplicationNum" },
            new SqlDbType[] { SqlDbType.Int, SqlDbType.Int });

            asset.AssetNum = (int)PDUtility.nullToNullInt(dt.Rows[0]["AssetNum"]);

            return asset;
        }
    }

}
