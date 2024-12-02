using Matrix.Data.Exceptions;
using Matrix.Display;
using Matrix.WebServices;

namespace Matrix;

public class MatrixMain
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Environment.CurrentDirectory, "data"))
            .AddJsonFile("matrix_settings.json")
            .Build();

        MatrixUpdater matrixUpdater;
        try
        {
            matrixUpdater = new MatrixUpdater(configuration);
        }
        catch (ConfigurationException ex)
        {
            await Console.Error.WriteLineAsync($"Could not parse configuration.\n{ex.Message}");
            return;
        }

        WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration);
        Thread thread = new Thread(webApp.Run);
        thread.Start();

        using (matrixUpdater)
        {
            int previousSecond = -1;
            while (true)
            {
                var time = DateTime.Now;
                if (previousSecond != time.Second)
                {
                    previousSecond = time.Second;

                    matrixUpdater.HandleUpdateLoop(time);
                }
        
                Thread.Sleep(matrixUpdater.GetUpdateInterval());
            }
        }
    }
}