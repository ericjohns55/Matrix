using Matrix.DataModels;

namespace Matrix.WebServices.Services;

public interface IMatrixService
{
    public string? GetTestData();
    public void SetData(string data);
    public Task<List<TestData>>  GetTestDataList();
}