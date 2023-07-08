namespace PriceDigestAPI.Models.GetRequests
{
    public class BasicGetRequest
    {
        public string? VinNum { get; set; }
        public string? VehicleCategory { get; set; }
        public string? VehicleYear { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
    }
}
