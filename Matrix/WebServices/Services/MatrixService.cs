using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Utilities;
using Matrix.WebServices.Clients;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class MatrixService : IMatrixService
{
    private MatrixContext _matrixContext;
    private IConfiguration _configuration;
    
    public MatrixService(MatrixContext context, IConfiguration configuration)
    {
        _matrixContext = context;
        _configuration = configuration;
    }

    public async Task<Dictionary<string, string>> UpdateVariables()
    {
        var weatherUrl = _configuration.GetValue<string>(ConfigConstants.WeatherUrl);

        if (weatherUrl != null)
        {
            using (var weatherClient = new WeatherClient(weatherUrl))
            {
                ProgramState.Weather = await weatherClient.GetWeather();
                ProgramState.UpdateVariables();
            }
        }

        return ProgramState.CurrentVariables;
    }

    public ClockFace AddNewClockFace(ClockFace newClockFace)
    {
        return new ClockFace();
    }

    public ClockFace GetClockFace(string name)
    {
        return new ClockFace();
    }

    public string DeleteClockFace(string name)
    {
        return string.Empty;
    }
}