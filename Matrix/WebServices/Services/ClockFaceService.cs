using Matrix.Data.Models;
using Matrix.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class ClockFaceService : IClockFaceService
{
    private readonly MatrixContext _matrixContext;

    public ClockFaceService(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }
    
    public Task<List<ClockFace>> GetAllClockFaces(SearchFilter filter = SearchFilter.Active)
    {
        if (filter == SearchFilter.AllResults)
        {
            return _matrixContext.ClockFace.ToListAsync();
        }
        
        var searchForDeleted = filter == SearchFilter.Deleted;

        return _matrixContext.ClockFace.Where(face => face.Deleted == searchForDeleted).ToListAsync();
    }

    public Task<ClockFace?> GetClockFace(int faceId)
    {
        return _matrixContext.ClockFace.FirstOrDefaultAsync(face => face.Id == faceId);
    }

    public async Task<ClockFace?> UpdateClockFace(int faceId, ClockFace newFace)
    {
        _matrixContext.ClockFace.Update(newFace);
        await _matrixContext.SaveChangesAsync();
        return await GetClockFace(faceId);
    }

    public async Task<ClockFace?> AddClockFace(ClockFace clockFace)
    {
        await _matrixContext.ClockFace.AddAsync(clockFace);
        await _matrixContext.SaveChangesAsync();
        
        return await _matrixContext.ClockFace.FirstOrDefaultAsync(face => face.Id == clockFace.Id);
    }

    public async Task<int> RemoveClockFace(int faceId)
    {
        var clockFace = _matrixContext.ClockFace.FirstOrDefault(face => face.Id == faceId);

        if (clockFace == null)
        {
            return -1;
        }
        
        clockFace.Deleted = true;
        _matrixContext.ClockFace.Remove(clockFace);
        await _matrixContext.SaveChangesAsync();

        return clockFace.Id;
    }
}