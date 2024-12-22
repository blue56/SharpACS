using System.Text.Json.Serialization;
using SharpACS;

[JsonDerivedType(typeof(SummaryResponse), typeDiscriminator: "MonthSummary")]
public abstract class Response
{

}
