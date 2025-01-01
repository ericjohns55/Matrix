using Matrix.Data.Exceptions;
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

    public async Task<ClockFace> GetClockFace(int faceId)
    {
        var clockFace = await _matrixContext.ClockFace.FirstOrDefaultAsync(face => face.Id == faceId);

        if (clockFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find clock face with id {faceId}");
        }

        return clockFace;
    }

    public async Task<ClockFace> UpdateClockFace(int faceId, ClockFace updatedFace)
    {
        var originalFace = await GetClockFace(faceId);

        if (originalFace == null)
        {
            throw new MatrixEntityNotFoundException($"Clock face with ID {faceId} was not found");
        }

        if (updatedFace == null)
        {
            throw new ArgumentNullException($"{updatedFace} cannot be null");
        }
        
        await _matrixContext.SaveChangesAsync();
        
        originalFace.Name = updatedFace.Name;
        originalFace.TextLines = updatedFace.TextLines;
        originalFace.TimePeriods = updatedFace.TimePeriods;
        
        _matrixContext.ClockFace.Update(originalFace);
        await _matrixContext.SaveChangesAsync();
        
        return await GetClockFace(faceId);
    }

    public async Task<ClockFace> AddClockFace(ClockFace clockFace)
    {
        if (clockFace == null)
        {
            throw new ArgumentNullException($"{clockFace} cannot be null");
        }
        
        await _matrixContext.ClockFace.AddAsync(clockFace);
        await _matrixContext.SaveChangesAsync();
        
        return await GetClockFace(clockFace.Id);
    }

    public async Task<int> RemoveClockFace(int faceId)
    {
        var clockFace = _matrixContext.ClockFace.FirstOrDefault(face => face.Id == faceId);

        if (clockFace == null)
        {
            throw new MatrixEntityNotFoundException($"Clock face with ID {faceId} was not found");
        }
        
        clockFace.Deleted = true;
        _matrixContext.ClockFace.Update(clockFace);
        await _matrixContext.SaveChangesAsync();

        return clockFace.Id;
    }

    public async Task<ClockFace> RestoreClockFace(int faceId)
    {
        var clockFace = _matrixContext.ClockFace.FirstOrDefault(face => face.Id == faceId);

        if (clockFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find clock face with ID {faceId}");
        }
        
        clockFace.Deleted = false;
        _matrixContext.ClockFace.Update(clockFace);
        await _matrixContext.SaveChangesAsync();

        return clockFace;
    }
}