using System.Text.Json.Serialization;
using SharpACS;

[JsonDerivedType(typeof(SummaryRequest), typeDiscriminator: "Summary")]
public abstract class Request
{

}
