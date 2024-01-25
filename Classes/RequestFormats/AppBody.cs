namespace PriceDigestAPI
{
    public class AppBody
    {
        public AppBody(int subscriberId, int applicationNum, int userNum, string CompanyDB)
        {
            this.subscriberId = subscriberId;
            this.applicationNum = applicationNum;
            this.userNum = userNum;
            this.CompanyDB = CompanyDB;
        }

        public int subscriberId { get; set; }
        public int applicationNum { get; set; }
        public int userNum { get; set; }
        public string CompanyDB { get; set; }
    }
}
