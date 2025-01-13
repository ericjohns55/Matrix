using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices;

namespace Matrix;

public class MatrixMain
{
    public static MatrixUpdater MatrixUpdater;
    
    private static bool _matrixLoopRunning = true;
    
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Environment.CurrentDirectory, "Data"))
            .AddJsonFile("matrix_settings.json")
            .Build();
        
        await ApiKeyHelper.LoadOrGenerateApiKey();
        
        try
        {
            MatrixUpdater = new MatrixUpdater(configuration);
        }
        catch (ConfigurationException ex)
        {
            await Console.Error.WriteLineAsync($"Could not parse configuration.\n{ex.Message}");
            return;
        }
        
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            _matrixLoopRunning = false;
            eventArgs.Cancel = true;
        };
        
        WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration, MatrixUpdater.FontsPath);
        Thread thread = new Thread(() => webApp.Run(MatrixUpdater.GetServerUrl()));
        thread.Start();
        
        using (MatrixUpdater)
        {
            MatrixUpdater.CurrentClockFace = await MatrixUpdater.MatrixClient.GetClockFaceForTime(TimePayload.Now());
            
            ProgramState.Weather = await MatrixUpdater.WeatherClient?.GetWeather()!;
            ProgramState.UpdateVariables();
            
            int previousSecond = -1;
            while (_matrixLoopRunning)
            {
                var time = DateTime.Now;
                if (previousSecond != time.Second)
                {
                    previousSecond = time.Second;

                    MatrixUpdater.HandleUpdateLoop(time);
                }
        
                Thread.Sleep(MatrixUpdater.GetUpdateInterval());
            }
        }
    }
}