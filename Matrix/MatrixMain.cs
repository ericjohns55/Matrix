using Matrix.Data.Models;
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

        WebApplication webApp = await MatrixServer.CreateWebServer(args, configuration);
        webApp.Run();
        
        // Thread thread = new Thread(webApp.Run);
        // thread.Start();
        
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