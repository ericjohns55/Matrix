using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Matrix.Data.Models;

public class TimePayload
{
    public int Hour { get; init; }
    public int Minute { get; init; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public DayOfWeek DayOfWeek { get; init; }

    public new string ToString()
    {
        return $"{DayOfWeek.ToString()} - {Hour}:{Minute:D2}";
    }

    public static TimePayload Now()
    {
        var time = DateTime.Now;

        return new TimePayload()
        {
            Hour = time.Hour,
            Minute = time.Minute,
            DayOfWeek = time.DayOfWeek
        };
    }
}