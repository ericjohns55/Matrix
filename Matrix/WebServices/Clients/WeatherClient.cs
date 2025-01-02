using Matrix.Data.Models;
using Matrix.Data.Utilities;
using Matrix.Utilities;
using Newtonsoft.Json.Linq;

namespace Matrix.WebServices.Clients;

public class WeatherClient : WebClient
{
    private Uri _weatherUri;

    public WeatherClient(string weatherUri)
    {
        _weatherUri = new Uri(weatherUri);
    }
    
    public Task<WeatherModel> GetWeather()
    {
        return ExecuteRequest((client) => client.GetAsync(_weatherUri),
            httpResponseMessage => HandleResponseAsString(httpResponseMessage)
                .OnSuccess(weatherJson =>
                {
                    var weatherObject = JObject.Parse(weatherJson);
                    
                    return new WeatherModel()
                    {
                        CurrentForecast = FromStringOrEmpty(weatherObject["current"]?["weather"]?[0]?["description"]),
                        CurrentForecastShort = FromStringOrEmpty(weatherObject["current"]?["weather"]?[0]?["main"], true),
                        DayForecast = FromStringOrEmpty(weatherObject["daily"]?[0]?["weather"]?[0]?["main"], true),
                        Temp = FromFloatOrEmpty(weatherObject["current"]?["temp"]),
                        TempLow = FromFloatOrEmpty(weatherObject["daily"]?[0]?["temp"]?["min"]),
                        TempHigh = FromFloatOrEmpty(weatherObject["daily"]?[0]?["temp"]?["max"]),
                        RealFeel = FromFloatOrEmpty(weatherObject["current"]?["feels_like"]),
                        WindSpeed = FromFloatOrEmpty(weatherObject["current"]?["wind_speed"], 1),
                        Humidity = FromFloatOrEmpty(weatherObject["current"]?["humidity"])
                    };
                }));
    }

    private string FromStringOrEmpty(JToken? token, bool remapTStorms = false)
    {
        if (token == null)
        {
            return WeatherModel.MissingStringValue;
        }

        var deserializedValue = token.ToObject<string>();

        if (deserializedValue == null)
        {
            return WeatherModel.MissingStringValue;
        }

        if (remapTStorms && deserializedValue == VariableConstants.ThunderKey)
        {
            deserializedValue = VariableConstants.ThunderReplace;
        }

        return deserializedValue;
    }

    private float FromFloatOrEmpty(JToken? token, int decimalPlaces = 0)
    {
        if (token == null)
        {
            return WeatherModel.MissingFloatValue;
        }

        var deserializedValue = token.ToObject<float?>();

        if (deserializedValue == null)
        {
            return WeatherModel.MissingFloatValue;
        }
        
        return (float) Math.Round(deserializedValue.Value, decimalPlaces);
    }
}