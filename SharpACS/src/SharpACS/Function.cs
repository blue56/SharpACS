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

            MonthSummaryResponse response = new MonthSummaryResponse();
            response.S3Bucketname = r.S3Bucketname;
            response.S3Path = r.S3KeyPattern;
            response.Region = r.Region;

            return response;
        }
        else if (Request is AWSCostExplorerRequest)
        {
/*            
            AWSCostExplorer ce = new AWSCostExplorer();

            AWSCostExplorerRequest aer = (AWSCostExplorerRequest)Request;
            var costList = ce.Execute(aer);

            // Write json file to S3
            // Write report-<year>-<month>.json
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(costList, options);
            var stream = GenerateStreamFromString(jsonString);

            SaveFile(aer.Region, aer.S3Bucketname,
                aer.S3Path, stream, "application/json");

            SharpACSResponse response = new SharpACSResponse();
            response.S3Bucketname = aer.S3Bucketname;
            response.S3Path = aer.S3Path;
            response.Region = aer.Region;
            response.Year = aer.Year;
            response.Month = aer.Month;

            return response;
 */ 
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
