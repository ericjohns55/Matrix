using Matrix.Data.Models;
using Matrix.Data.Types;

namespace Matrix.WebServices.Services;

public interface IClockFaceService
{
    public Task<List<ClockFace>> GetAllClockFaces(SearchFilter filter = SearchFilter.Active);
    public Task<ClockFace?> GetClockFace(int faceId);
    public Task<ClockFace?> UpdateClockFace(int faceId, ClockFace newFace);
    public Task<ClockFace?> AddClockFace(ClockFace clockFace);
    public Task<int> RemoveClockFace(int faceId);
}