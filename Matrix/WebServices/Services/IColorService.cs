using Matrix.Data.Models;
using Matrix.Data.Types;

namespace Matrix.WebServices.Services;

public interface IColorService
{
    public Task<List<MatrixColor>>GetMatrixColors(SearchFilter filter = SearchFilter.Active);
    public Task<MatrixColor?> GetMatrixColor(int colorId);
    public Task<MatrixColor?> UpdateMatrixColor(int colorId, MatrixColor color);
    public Task<MatrixColor?> AddMatrixColor(MatrixColor color);
    public Task<int> RemoveMatrixColor(int colorId);
}