using Matrix.Data.Models;
using Matrix.Data.Models.TimeValidation;
using Matrix.Data.Types;

namespace Matrix.WebServices.Services;

public interface IClockFaceService
{
    public Task<List<ClockFace>> GetAllClockFaces(SearchFilter filter = SearchFilter.Active);
    public Task<ClockFace> GetClockFaceForTime(TimePayload timePayload);
    public Task<ClockFace> GetClockFace(int faceId);
    public Task<ClockFace> UpdateClockFace(int faceId, ClockFace updatedFace);
    public Task<ClockFace> AddClockFace(ClockFace clockFace);
    public Task<int> RemoveClockFace(int faceId);
    public Task<ClockFace> RestoreClockFace(int faceId);
    public Task<ValidationResponse> ValidateClockFaceTimePeriods();
    public Task<ClockFace> GetTimerClockFace();
}