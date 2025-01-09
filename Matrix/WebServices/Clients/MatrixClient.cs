using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Utilities;

namespace Matrix.WebServices.Clients;

public class MatrixClient : WebClient
{
    private Uri _serverUri;
    
    public MatrixClient(string serverUri)
    {
        _serverUri = new Uri(serverUri);
    }

    public Task<Dictionary<string, object?>> GetConfiguration()
    {
        Uri uri = BuildUri(_serverUri.AddToPath("matrix", "config"));

        return ExecuteRequest((client) => client.GetAsync(uri),
            httpResponseMessage => HandleJsonResponse<Dictionary<string, object?>>(httpResponseMessage)
                .OnSuccess(config => config));
    }

    public Task<ClockFace> GetClockFace()
    {
        Uri uri = BuildUri(_serverUri.AddToPath("matrix", "face"));

        return ExecuteRequest((client) => client.GetAsync(uri),
            httpResponseMessage => HandleJsonResponse<ClockFace>(httpResponseMessage)
                .OnSuccess(clockFace => clockFace));
    }

    public Task<ClockFace> GetClockFaceForTime(TimePayload timePayload)
    {
        Uri uri = BuildUri(_serverUri.AddToPath("clockface", "at"));

        return ExecuteRequest((client) => client.PostAsync(uri, SerializeHttpContent(timePayload)),
            httpResponseMessage => HandleJsonResponse<MatrixResponse<ClockFace>>(httpResponseMessage)
                .OnSuccess(matrixResponse => matrixResponse.Data));
    }

    public Task<bool> SendUpdate()
    {
        Uri uri = BuildUri(_serverUri.AddToPath("matrix", "update"));

        return ExecuteRequest((client) => client.PostAsync(uri, SerializeHttpContent(string.Empty)),
            httpResponseMessage => HandleJsonResponse<bool>(httpResponseMessage)
                .OnSuccess(response => response));
    }
}