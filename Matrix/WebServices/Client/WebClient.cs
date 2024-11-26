using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Matrix.Utilities;

namespace Matrix.WebServices;

public class WebClient
{
    private ILogger<WebClient> _logger { get; }
    private HttpClient _httpClient;

    public WebClient()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebClient>();
        _httpClient = new HttpClient();
    }
    
    protected readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    protected HttpClient GetClient() => _httpClient;
    
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
        HttpResponseMessage? response = null;

        try
        {
            response = await executeFunction(_httpClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        try
        {
            if (response != null)
            {
                return await resultFunction(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return null;
    }

    protected HttpContent SerializeHttpContent<T>(T value)
    {
        return new StringContent(JsonSerializer.Serialize(value, _jsonOptions), Encoding.UTF8, "application/json");
    }

    protected Task<T> DeserializeHttpContent<T>(HttpResponseMessage response)
    {
        return response.Content.ReadAsByteArrayAsync().OnSuccess(responseBytes => (T) JsonSerializer.Deserialize<T>(responseBytes, _jsonOptions));
    }

    protected async Task<T> HandleJsonResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await DeserializeHttpContent<T>(response);
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();
            
        }
    }
}