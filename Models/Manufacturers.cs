namespace PriceDigestAPI.Models
{
    public class Manufacturers
    {
        public int ManufacturerID { get; set; }
        public string ManufacturerName { get; set; }
        public List<string>? ManufacturerAliases { get; set; }
    }
}
