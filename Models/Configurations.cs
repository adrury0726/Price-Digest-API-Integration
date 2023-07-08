namespace PriceDigestAPI
{
    public class Configurations
    {
        public int ModelId { get; set; } //Can be ModelID's that are the same
        public string ModelName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
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
        public int ConfigurationId { get; set; } //Unique Identifier
        public string VinModelNumber { get; set; }
        public string ModelYear { get; set; }
        public List<DisplaySpec> DisplaySpecs { get; set; }
    }

    public class DisplaySpec
    {
        public string? SpecName { get; set; }
        public string? SpecValue { get; set; }
        public string? SpecNameFriendly { get; set; }
        public string? SpecUom { get; set; }
        public string? SpecDescription { get; set; }
        public string? SpecFamily { get; set; }
    }
}
