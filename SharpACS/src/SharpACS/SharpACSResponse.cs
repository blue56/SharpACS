namespace SharpACS
{
    public class SharpACSResponse
    {
        public string Region { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string S3Bucketname { get; set; }
        public string S3Path { get; set; }
    }
}