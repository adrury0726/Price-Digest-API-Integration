namespace PriceDigestAPI.Models.GetRequests
{
    public class IncompleteTruckRequest
    {
        public string? Category { get; set; }
        public string? ModelYear { get; set; }
        public string? ModelName { get; set; }
    }
}
