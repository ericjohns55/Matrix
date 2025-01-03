namespace Matrix.Data.Models.Web;

public class WeatherModel
{
    public string CurrentForecast { get; init; }
    public string CurrentForecastShort { get; init; }
    public string DayForecast { get; init; }
    public float Temp { get; init; }
    public float TempLow { get; init; }
    public float TempHigh { get; init; }
    public float RealFeel { get; init; }
    public float WindSpeed { get; init; }
    public float Humidity { get; init; }

    public static readonly string MissingStringValue = "NONE";
    public static readonly float MissingFloatValue = -1;
    public static WeatherModel Empty { get; } = new WeatherModel()
    {
        CurrentForecast = MissingStringValue,
        CurrentForecastShort = MissingStringValue,
        DayForecast = MissingStringValue,
        Temp = MissingFloatValue,
        TempLow = MissingFloatValue,
        TempHigh = MissingFloatValue,
        RealFeel = MissingFloatValue,
        WindSpeed = MissingFloatValue,
        Humidity = MissingFloatValue,
    };
}