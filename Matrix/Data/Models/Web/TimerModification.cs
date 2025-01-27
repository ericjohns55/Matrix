using Matrix.Data.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Matrix.Data.Models.Web;

public class TimerModification
{
    public int HourAmount { get; init; }
    public int MinuteAmount { get; init; }
    public int SecondAmount { get; init; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public ModificationType ModificationType { get; init; }
    
    public int TickCount => HourAmount * 3600 + MinuteAmount * 60 + SecondAmount;
}