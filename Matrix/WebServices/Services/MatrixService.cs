using Matrix.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class MatrixService : IMatrixService
{
    private static string? _data;
    private MatrixContext _matrixContext;
    
    public MatrixService(MatrixContext context)
    {
        _matrixContext = context;
    }

    public TestData GetTestData() => new TestData() { Data = _data};
    
    public void SetData(string data) => _data = data;

    // public async Task<List<TestData>> GetTestDataList()
    // {
    //     return new List<TestData>();
    // }
    //
    // public async Task AddTestDataAsync(TestData testData)
    // {
    //     return Task.CompletedTask;
    // }
}