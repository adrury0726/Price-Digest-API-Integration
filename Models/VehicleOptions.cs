namespace PriceDigestAPI
{
    public class VehicleOptions
    {
        public int ClassificationID { get; set; }
        public string ClassificationName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubtypeID { get; set; }
        public string SubtypeName { get; set; }
        public int SizeClassID { get; set; }
        public string SizeClassName { get; set; }
        public int SizeClassMin { get; set; }
        public int SizeClassMax { get; set; }
        public string SizeClassUom { get; set; }
        public int ModelYear { get; set; }
        public List<OptionDetails> Options { get; set; }
    }

    public class OptionDetails
    {
        public int OptionFamilyID { get; set; }
        public string OptionFamilyName { get; set; }
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
        public string OptionMSRP { get; set; }
    }
}
