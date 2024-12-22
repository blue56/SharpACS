using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace SharpACS
{
    public class Helpers
    {
        public static void SaveFile(string Region, string S3Bucketname, string S3Path, Stream Stream, string ContentType)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = S3Bucketname,
                Key = S3Path,
                ContentType = ContentType,
                InputStream = Stream
            };

            var region = RegionEndpoint.GetBySystemName(Region);

            var _client = new AmazonS3Client(region);

            _client.PutObjectAsync(putRequest).Wait();
        }
    }
}
