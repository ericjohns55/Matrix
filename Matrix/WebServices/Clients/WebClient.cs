using System.Collections.Specialized;
using System.Text;
using System.Web;
using Matrix.Data.Exceptions;
using Matrix.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace Matrix.WebServices.Clients;

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

    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = new List<JsonConverter>() { new StringEnumConverter() }
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
        return new StringContent(JsonConvert.SerializeObject(value, _jsonSettings), Encoding.UTF8, "application/json");
    }

    private Task<T> DeserializeHttpContent<T>(HttpResponseMessage response)
    {
        return response.Content.ReadAsStringAsync().OnSuccess(responseString => (T) JsonConvert.DeserializeObject<T>(responseString, _jsonSettings));
    }

    protected async Task<string> HandleResponseAsString(HttpResponseMessage response)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new WebException((int) response.StatusCode, stringContent);
        }

        if (stringContent == null)
        {
            throw new WebException(500, "Could not read JSON data");
        }

        return stringContent;
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