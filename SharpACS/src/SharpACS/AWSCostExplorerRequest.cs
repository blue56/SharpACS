namespace SharpACS
{
    public class AWSCostExplorerRequest
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string CostCategoryName { get; set; }
        public string Metric { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool Tax { get; set; }

        public string S3Bucketname { get; set; }
        public string S3Path { get; set; }
    }
}