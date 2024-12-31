using System.Diagnostics;
using Matrix.Data.Exceptions;
using Matrix.Display;
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
        
        WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration);
        Thread thread = new Thread(() => webApp.Run(MatrixUpdater.GetServerUrl()));
        thread.Start();

        using (var client = new MatrixClient(MatrixUpdater.GetServerUrl()))
        {
            await client.SendUpdate();
        }

        using (MatrixUpdater)
        {
            int previousSecond = -1;
            while (_matrixLoopRunning)
            {
                var time = DateTime.Now;
                if (previousSecond != time.Second)
                {
                    previousSecond = time.Second;

                    var watch = Stopwatch.StartNew();
                    MatrixUpdater.HandleUpdateLoop(time);
                    watch.Stop();
                    Console.WriteLine($"Time elapsed: {watch.ElapsedMilliseconds} ms");
                }
        
                Thread.Sleep(MatrixUpdater.GetUpdateInterval());
            }
        }
    }
}