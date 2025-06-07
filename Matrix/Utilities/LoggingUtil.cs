using System.Text;

namespace Matrix.Utilities;

public class LoggingUtil
{
    public static async Task ExecuteWithLogging(Func<Task> function)
    {
        string loggingFolderPath = Path.Combine(MatrixMain.DataFolderPath, "logs");

        if (!Directory.Exists(loggingFolderPath))
        {
            Directory.CreateDirectory(loggingFolderPath);
        }
        
        try
        {
            await function();
        }
        catch (Exception ex)
        {
            string fileName = $"matrix_log-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            string currentLogFilePath = Path.Combine(loggingFolderPath, fileName);

            var stringBuilder = new StringBuilder($"Exception occurred at {DateTime.Now}:\n");
            stringBuilder.Append($"Message: {ex.Message}\n");
            stringBuilder.Append($"Stack Trace: {ex.StackTrace}\n");
            stringBuilder.Append($"Base Exception: {ex.GetBaseException().StackTrace}\n");

            try
            {
                using (var fileStream = File.Create(currentLogFilePath))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        await streamWriter.WriteAsync(stringBuilder.ToString());
                    }
                }
            }
            catch (Exception writingException)
            {
                Console.WriteLine($"Could not write log to file: {writingException.Message}");
                Console.WriteLine(writingException.StackTrace);
                throw;
            }

            throw;
        }
    }
}