using Matrix.Data.Models;

namespace Matrix.WebServices.Services;

public interface IClockFaceService
{
    // public Task<List<ClockFace>> GetClockFaces();
    // public Task<ClockFace> AddClockFace(ClockFace face);
    
    public Task<List<MatrixColor>>GetMatrixColors();
    public Task<MatrixColor?> GetMatrixColor(int colorId);
    public Task<MatrixColor?> GetMatrixColor(string colorName);
    public Task<MatrixColor?> UpdateMatrixColor(int colorId, MatrixColor color);
    public Task<MatrixColor?> AddMatrixColor(MatrixColor color);
    public Task<int> RemoveMatrixColor(int colorId);
}