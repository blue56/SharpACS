using System.Text.Json.Serialization;
using SharpACS;

[JsonDerivedType(typeof(MonthSummaryResponse), typeDiscriminator: "MonthSummary")]
public abstract class Response
{

}
