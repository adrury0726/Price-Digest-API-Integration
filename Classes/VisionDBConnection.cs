using System.Data;
using System.Data.SqlClient;

//This class is where we are creating our VIsion Database Connection. It's also where we'll build our methods for when we're executing sprocs.

namespace PriceDigestAPI
{
    public interface IDBConnection
    {
        public void ExecSproc(ref DataTable data, string sproc, object[] values, string[] parameters = null, SqlDbType[] sqlDbTypes = null, bool mergeResults = false, bool searchSchema = true, string sqlServerCon = null);
        public void ExecSproc(string sproc, object[] values, string[] parameters = null, SqlDbType[] sqlDbTypes = null, bool mergeResults = false, bool searchSchema = true, string sqlServerCon = null);
        public string LookupItemData(int subscriberId, int companyNum, string itemCategory, string itemName, string itemSuperCategory = "0", bool strictMatch = false, bool returnItemNote = false);
    }
    public class VisionDBConnection : IDBConnection
    {   
        private string _connectionString;
        public VisionDBConnection()
        {
            _connectionString = "server = server-sql-vt; database = TandemVision; Integrated Security = True;";
        }
        public void ExecSproc(ref DataTable data, string sproc, object[] values, string[] parameters = null, SqlDbType[] sqlDbTypes = null, bool mergeResults = false, bool searchSchema = true, string sqlServerCon = null)
        {
            //this method is very much a barebones version of the vision utility method. It's just meant to get the job done during development
            SqlConnection sqlCon = new SqlConnection(_connectionString);
            SqlDataAdapter sqldta = new SqlDataAdapter(sproc, sqlCon);
            sqldta.SelectCommand.CommandType = CommandType.StoredProcedure;
            for (var i = 0; i < parameters.Length; i++)
            {
                sqldta.SelectCommand.Parameters.Add("@" + parameters[i], sqlDbTypes[i]).Value = values[i];
            }
            sqldta.Fill(data);
        }
        public void ExecSproc(string sproc, object[] values, string[] parameters = null, SqlDbType[] sqlDbTypes = null, bool mergeResults = false, bool searchSchema = true, string sqlServerCon = null)
        {
            DataTable dt = new DataTable();
            ExecSproc(ref dt, sproc, values, parameters, sqlDbTypes, mergeResults, searchSchema, sqlServerCon);
        }
        public string LookupItemData(int subscriberId, int companyNum, string itemCategory, string itemName, string itemSuperCategory = "0", bool strictMatch = false, bool returnItemNote = false)
        {
            //Again, very barebones
            DataTable dt = new DataTable();

            ExecSproc(ref dt, "dbo.sLookupValue", new Object[] { subscriberId, companyNum, itemCategory, itemName, itemSuperCategory }, new string[] { "SubscriberID", "CompanyNum", "ItemCategory", "ItemName", "ItemSuperCategory" }, new SqlDbType[] { SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar });
            return PDUtility.NullToString(dt.Rows[0]["ItemData"]);
        }
    }
}
