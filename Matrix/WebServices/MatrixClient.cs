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
    
    public Task<TestData> GetTestData()
    {
        Uri uri = BuildUri(_baseUri.AddToPath("matrix", "test"));
        
        return ExecuteRequest((client) => client.GetAsync(uri),
            httpResponseMessage => HandleJsonResponse<TestData>(httpResponseMessage))
                .OnSuccess(task => task);
    }

    public Task<TestData> PostTestData(TestData model)
    {
        Uri uri = BuildUri(_baseUri.AddToPath("matrix", "test"));
        
        return ExecuteRequest(
            (client) => client.PostAsync(uri, SerializeHttpContent(model)),
            httpResponseMessage => HandleJsonResponse<TestData>(httpResponseMessage)
                .OnSuccess(task => task));
    }
}