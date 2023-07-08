namespace PriceDigestAPI
{
    public class Adjustments
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public List<string>? ModelAliases { get; set; }
        public int ManufacturerID { get; set; }
        public string ManufacturerName { get; set; }
        public List<string>? ManufacturerAliases { get; set; }
        public int ClassificationID { get; set; }
        public string ClassificationName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubtypeID { get; set; }
        public string SubTypeName { get; set; }
        public int SizeClassID { get; set; }
        public string SizeClassName { get; set; }
        public int SizeClassMin { get; set; }
        public int SizeClassMax { get; set; }
        public string SizeClassUom { get; set; }
        public string ModelYear { get; set; }
        public int ConfigurationID { get; set; }
        public string RevisionDate { get; set; }
        public int MSRP { get; set; }
        public int? unadjustedLow { get; set; }
        public int? unadjustedHigh { get; set; }
        public int unadjustedFinance { get; set; }
        public int unadjustedRetail { get; set; }
        public int unadjustedWholesale { get; set; }
        public int? unadjustedTradeIn { get; set; }
        public int adjustedFinance { get; set; }
        public int adjustedWholesale { get; set; }
        public int adjustedRetail { get; set; }
        public int? adjustedTradeIn { get; set; }
        public int? adjustedHigh { get; set; }
        public int? adjustedLow { get; set; }
    }
}
