using Matrix.DataModels;
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

    public string? GetTestData() => _data;
    
    public void SetData(string data) => _data = data;

    public async Task<List<TestData>> GetTestDataList()
    {
        return await _matrixContext.TestData.ToListAsync();
    }
}