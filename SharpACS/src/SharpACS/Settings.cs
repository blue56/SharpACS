using Amazon;

namespace SharpACS;

public class Settings
{
    private static RegionEndpoint _region;
    private static string _bucketname;

    public static void Initialize()
    {
        // Get from environment variable

        string Region = Environment.GetEnvironmentVariable("Region");
        _region = RegionEndpoint.GetBySystemName(Region);

        //
        _bucketname = Environment.GetEnvironmentVariable("Bucketname");

    }

    public static RegionEndpoint GetRegion()
    {
        return _region;
    }

    public static string GetBucketName()
    {
        return _bucketname;
    }
}