using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SharpACS;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public Response FunctionHandler(Request Request, ILambdaContext context)
    {
        // Execute request
        if (Request is SummaryRequest)
        {
            var r = (SummaryRequest)Request;
            r.Run();

            SummaryResponse response = new SummaryResponse();
            response.S3Bucketname = r.S3Bucketname;
            response.S3Path = r.S3KeyPattern;
            response.Region = r.Region;

            return response;
        }

        return null;
    }

    public void SaveFile(string Region, string S3Bucketname, string S3Path, Stream Stream, string ContentType)
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

    public static Stream GenerateStreamFromString(string s)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(s));
    }
}
