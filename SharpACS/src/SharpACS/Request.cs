using System.Text.Json.Serialization;
using SharpACS;

[JsonDerivedType(typeof(SummaryRequest), typeDiscriminator: "Summary")]
[JsonDerivedType(typeof(AWSCostExplorerRequest), typeDiscriminator: "Explorer")]
public abstract class Request
{

}
