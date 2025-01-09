namespace Matrix.Utilities;

public class ApiKeyHelper
{
    public static string ApiKey { get; internal set; }
    
    public static async Task LoadOrGenerateApiKey()
    {
        string dataFolderPath = Path.Combine(Environment.CurrentDirectory, "Data");
        string apiKeyPath = Path.Combine(dataFolderPath, "api_key");
        string apiKey;
        
        if (!File.Exists(apiKeyPath))
        {
            using (var newFile = File.Create(apiKeyPath))
            {
                using (var streamWriter = new StreamWriter(newFile))
                {
                    ApiKey = Guid.NewGuid().ToString().Replace("-", "");
                    await streamWriter.WriteAsync(ApiKey);
                }
            }
        }
        else
        {
            using (var keyFile = File.OpenRead(apiKeyPath))
            {
                using (var streamReader = new StreamReader(keyFile))
                {
                    ApiKey = await streamReader.ReadToEndAsync();
                }
            }
        }
    }
}