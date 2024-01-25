namespace PriceDigestAPI
{
    //Properties being used for Verfication
    public class Verification
    {
        public int ModelID { get; set; }
        public string ModelName { get; set; }
        public List<string>? ModelAliases { get; set; }
        public int ManufacturerID { get; set; }
        public List<string>? ManufacturerAliases { get; set; }
        public string ManufacturerName { get; set; }
        public int ClassificationID { get; set; }
        public string ClassificationName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubTypeID { get; set; }
        public string SubTypeName { get; set; }
        public int SizeClassID { get; set; }
        public string SizeClassName { get; set; }
        public int SizeClassMin { get; set; }
        public int SizeClassMax { get; set; }
        public string SizeClassUom { get; set; }
        public string ConfigurationID { get; set; } // Unique Identifier
        public string ModelYear { get; set; }
        public string VinModelNumber { get; set; }
        public string VinManufacturerCode { get; set; }
        public string VinYearCode { get; set; }
        public string ShortVin { get; set; }
        public string? CicCode { get; set; }
        public string Brand { get; set;}

    }
}
