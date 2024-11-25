using Matrix.WebServices;

namespace Matrix;

public class MatrixMain
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        WebApplication webApp = await MatrixServer.CreateWebServer(args);
        Thread thread = new Thread(webApp.Run);
        thread.Start();
        
        for (int i = 0; i < int.MaxValue; i++)
        {
            Console.WriteLine($"Iter {i}");
            Thread.Sleep(1000);
        }
    }
}