namespace PriceDigestAPI
{
    public class AppBody
    {
        public AppBody(int subscriberId, int applicationNum, int userNum)
        {
            this.subscriberId = subscriberId;
            this.applicationNum = applicationNum;
            this.userNum = userNum;
        }

        public int subscriberId { get; set; }
        public int applicationNum { get; set; }
        public int userNum { get; set; }
    }
}
