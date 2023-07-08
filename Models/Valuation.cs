namespace PriceDigestAPI.Models
{
    public class Valuation
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public List<string> ModelAliases { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public List<string> ManufacturerAliases { get; set; }
        public int ClassificationId { get; set; }
        public string ClassificationName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubtypeId { get; set; }
        public string SubtypeName { get; set; }
        public int SizeClassId { get; set; }
        public string SizeClassName { get; set; }
        public int SizeClassMin { get; set; }
        public int SizeClassMax { get; set; }
        public string SizeClassUom { get; set; }
        public string ModelYear { get; set; }
        public int ConfigurationId { get; set; }
        public DateTime RevisionDate { get; set; }
        public int MSRP { get; set; }
        public int? UnadjustedLow { get; set; }
        public int? UnadjustedHigh { get; set; }
        public int? UnadjustedFinance { get; set; }
        public int? UnadjustedRetail { get; set; }
        public int? UnadjustedWholesale { get; set; }
        public int? UnadjustedTradeIn { get; set; }
        public string? FuelType { get; set; }
        public int? AdjustedFinance { get; set; }
        public int? AdjustedWholesale { get; set; }
        public int? AdjustedRetail { get; set; }
        public int? AdjustedTradeIn { get; set; }
        public int? AdjustedHigh { get; set; }
        public int? AdjustedLow { get; set; }
    }
}
