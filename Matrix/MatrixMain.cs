using System.Runtime.CompilerServices;
using System.Text;
using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix;

public class MatrixMain
{
    public static string ParseTime(DateTime now)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{now.Hour % 12}:");
        stringBuilder.Append($"{now.Minute.ToString().PadLeft(2, '0')}:");
        stringBuilder.Append($"{now.Second.ToString().PadLeft(2, '0')}");
        stringBuilder.Append(now.Hour > 11 ? " PM" : " AM");
        return stringBuilder.ToString();
    }
    
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Environment.CurrentDirectory, "data"))
            .AddJsonFile("matrix_settings.json")
            .Build();

        WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration);
        Thread thread = new Thread(webApp.Run);
        thread.Start();
        
        if (int.TryParse(configuration["UpdateInterval"], out int updateInterval))
        {
            int previousSecond = -1;
            while (true)
            {
                var time = DateTime.Now;
                if (previousSecond != time.Second)
                {
                    previousSecond = time.Second;

                    if (ProgramState.NeedsUpdate(time))
                    {
                        ProgramState.Update = false;
                        
                        if (ProgramState.State == MatrixState.Timer)
                        {
                            ProgramState.Timer.Tick();
                            
                            Console.WriteLine($"Ticks Left: {ProgramState.Timer.GetFormattedTimer()}");

                            if (ProgramState.Timer.HasEnded())
                            {
                                ProgramState.State = ProgramState.PreviousState;
                                ProgramState.PreviousState = MatrixState.Timer;
                                ProgramState.Update = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine(ParseTime(time));
                        }
                    }
                }
        
                Thread.Sleep(updateInterval);
            }
        }
        else
        {
            Console.WriteLine("Please specify a valid update interval in milliseconds.");
        }

        // using (MatrixClient matrixClient = new MatrixClient(configuration["ServerUrl"]))
        // {
        //     TestData testData = new TestData() { Data = "THIS IS NEW DATA" };
        //
        //     try
        //     {
        //         var result = await matrixClient.PostTestData(testData);
        //         Console.WriteLine($"Data sent: {result.Data}");
        //     }
        //     catch (WebException ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        //     
        //     try
        //     {
        //         var result = await matrixClient.GetTestData();
        //     
        //         Console.WriteLine($"Data received: {result.Data}");
        //     }
        //     catch (WebException ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //     }
        // }

    }
}