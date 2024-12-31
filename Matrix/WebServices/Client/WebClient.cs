using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Matrix.Utilities;

namespace Matrix.WebServices.Client;

public class WebClient : IDisposable
{
    private ILogger<WebClient> _logger { get; }
    private readonly HttpClient _httpClient;

    public WebClient()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebClient>();
        _httpClient = new HttpClient();
        
        var encodedKey = Encoding.UTF8.GetBytes(MatrixServer.ApiKey);
        var base64Key = Convert.ToBase64String(encodedKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Key}");
    }
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
    
    protected Uri BuildUri(Uri uri, NameValueCollection? queryString = null)
    {
        UriBuilder builder = new UriBuilder(uri);
        
        var parameters = HttpUtility.ParseQueryString(builder.Query);
        if (queryString != null)
        {
            parameters.Add(queryString);
        }

        builder.Query = parameters.ToString();
        return builder.Uri;
    }

    protected async Task<T> ExecuteRequest<T>(Func<HttpClient, Task<HttpResponseMessage>> executeFunction,
        Func<HttpResponseMessage, Task<T>> resultFunction)
    {
        HttpResponseMessage? response;

        try
        {
            response = await executeFunction(_httpClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }

        try
        {
            return await resultFunction(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    protected HttpContent SerializeHttpContent<T>(T value)
    {
        return new StringContent(JsonSerializer.Serialize(value, _jsonOptions), Encoding.UTF8, "application/json");
    }

    private Task<T> DeserializeHttpContent<T>(HttpResponseMessage response)
    {
        return response.Content.ReadAsByteArrayAsync().OnSuccess(responseBytes => (T) JsonSerializer.Deserialize<T>(responseBytes, _jsonOptions));
    }

    protected async Task<T> HandleJsonResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new WebException((int) response.StatusCode, content);
        }
        
        return await DeserializeHttpContent<T>(response);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}