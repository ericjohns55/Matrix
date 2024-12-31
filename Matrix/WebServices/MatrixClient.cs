using Matrix.Data.Models;
using Matrix.Utilities;
using Matrix.WebServices.Client;

namespace Matrix.WebServices;

public class MatrixClient : WebClient
{
    private Uri _baseUri;

    public MatrixClient(string baseUri)
    {
        _baseUri = new Uri(baseUri);
    }

    public Task<Dictionary<string, object?>> GetConfiguration()
    {
        Uri uri = BuildUri(_baseUri.AddToPath("matrix", "config"));

        return ExecuteRequest((client) => client.GetAsync(uri),
            httpResponseMessage => HandleJsonResponse<Dictionary<string, object?>>(httpResponseMessage)
                .OnSuccess(config => config));
    }

    public Task<ClockFace> GetClockFace()
    {
        Uri uri = BuildUri(_baseUri.AddToPath("matrix", "face"));

        return ExecuteRequest((client) => client.GetAsync(uri),
            httpResponseMessage => HandleJsonResponse<ClockFace>(httpResponseMessage)
                .OnSuccess(clockFace => clockFace));
    }

    public Task<bool> SendUpdate()
    {
        Uri uri = BuildUri(_baseUri.AddToPath("matrix", "update"));

        return ExecuteRequest((client) => client.PostAsync(uri, SerializeHttpContent(string.Empty)),
            httpResponseMessage => HandleJsonResponse<bool>(httpResponseMessage)
                .OnSuccess(response => response));
    }
}