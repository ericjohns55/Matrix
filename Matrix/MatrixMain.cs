using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Display;
using Matrix.Utilities;
using Matrix.WebServices;
using Matrix.GpioIntegrations;

namespace Matrix;

public class MatrixMain
{
    public static MatrixUpdater MatrixUpdater = null!;
    public static Integrations Integrations = null!;
    
    private static bool _matrixLoopRunning = true;
    
    public static string DataFolderPath => Path.Combine(Environment.CurrentDirectory, "Data");
    
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Environment.CurrentDirectory, "Data"))
            .AddJsonFile("matrix_settings.json")
            .Build();

        Integrations = Integrations.SetupIntegrations(configuration);
        
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
        
        await LoggingUtil.ExecuteWithLogging(async () =>
        {
            WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration, MatrixUpdater.FontsPath);
            Thread thread = new Thread(() => webApp.Run(MatrixUpdater.GetServerUrl()));
            thread.Start();
        
            using (MatrixUpdater)
            {
                ProgramState.Weather = await MatrixUpdater.WeatherClient?.GetWeather()!;
                ProgramState.UpdateVariables();
            
                MatrixUpdater.CurrentClockFace = await MatrixUpdater.MatrixClient.GetClockFaceForTime(TimePayload.Now());
            
                int previousSecond = -1;
                while (_matrixLoopRunning)
                {
                    // scrolling text allows variable updates
                    bool shouldHandleUpdateLoop = ProgramState.State == MatrixState.ScrollingText;
                
                    var time = DateTime.Now;
                    if (previousSecond != time.Second)
                    {
                        previousSecond = time.Second;
                        shouldHandleUpdateLoop = true;
                    }

                    if (shouldHandleUpdateLoop)
                    {
                        MatrixUpdater.HandleUpdateLoop(time);
                    }
        
                    Thread.Sleep(MatrixUpdater.GetUpdateInterval());
                }
            }
        });
    }
}