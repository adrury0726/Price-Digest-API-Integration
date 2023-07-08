namespace PriceDigestAPI.Models
{
    public class AllModels
    {
        public int ModelID { get; set; }
        public string ModelName { get; set; }
        public List<string>? ModelAliases { get; set; }
        public int ManufacturerID { get; set; }
        public string ManufacturerName { get; set; }
        public List<string>? ManufacturerAliases { get; set; }

    }
}
