using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Models.TimeValidation;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class ClockFaceService : IClockFaceService
{
    private readonly MatrixContext _matrixContext;
    private readonly int _minutesInDay = 24 * 60;

    public ClockFaceService(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }
    
    public Task<List<ClockFace>> GetAllClockFaces(SearchFilter filter = SearchFilter.Active)
    {
        if (filter == SearchFilter.AllResults)
        {
            return _matrixContext.ClockFace
                .Include(face => face.TimePeriods)
                .Include(face => face.TextLines).ThenInclude(line => line.Color)
                .Include(face => face.TextLines).ThenInclude(line => line.Font)
                .ToListAsync();
        }
        
        var searchForDeleted = filter == SearchFilter.Deleted;

        return _matrixContext.ClockFace.Where(face => face.Deleted == searchForDeleted)
            .Include(face => face.TimePeriods)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .ToListAsync();
    }

    public async Task<ClockFace> GetClockFace(int faceId)
    {
        var clockFace = await _matrixContext.ClockFace.Where(face => face.Id == faceId)
            .Include(face => face.TimePeriods)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .FirstOrDefaultAsync();

        if (clockFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find clock face with id {faceId}");
        }

        return clockFace;
    }

    public async Task<ClockFace> GetTimerClockFace()
    {
        var timerFace = await _matrixContext.ClockFace.Where(face => face.Name == WebConstants.TimerFace)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .FirstOrDefaultAsync();

        if (timerFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find timer clock face");
        }

        return timerFace;
    }

    public async Task<ClockFace> GetClockFaceForTime(TimePayload timePayload)
    {
        if (timePayload == null)
        {
            throw new ClockFaceException(WebConstants.TimePayloadNull);
        }
        
        var timePeriodsWithDay = await _matrixContext.TimePeriod
            .Where(timePeriod => timePeriod.DaysOfWeek.Contains(timePayload.DayOfWeek))
            .ToListAsync();
        
        var currentTimeInMinutes = timePayload.Hour * 60 + timePayload.Minute;

        var timePeriodsWithinTime = timePeriodsWithDay
            .Where(timePeriod => currentTimeInMinutes >= GetTimeInMinutes(timePeriod, true) &&
                                 currentTimeInMinutes < GetTimeInMinutes(timePeriod, false))
            .ToList();

        var clockFaces = timePeriodsWithinTime.Select(timePeriod => timePeriod.ClockFaceId)
            .Distinct()
            .ToList();

        if (clockFaces.Count == 0)
        {
            throw new MatrixEntityNotFoundException($"Could not find clock face for time {timePayload.ToString()}");
        }

        if (clockFaces.Count > 1)
        {
            throw new ClockFaceException(WebConstants.MultipleClockFaces);
        }

        return await GetClockFace(clockFaces.First());
    }

    private int GetTimeInMinutes(TimePeriod timePeriod, bool startTime)
    {
        if (startTime)
        {
            return timePeriod.StartHour * 60 + timePeriod.StartMinute;
        }
        
        return timePeriod.EndHour * 60 + timePeriod.EndMinute;
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
    
    public async Task<ValidationResponse> ValidateClockFaceTimePeriods()
    {
        var timePeriods = await _matrixContext.TimePeriod.ToListAsync();
        var validationFailures = new List<ValidationFailure>();

        foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
        {
            var timePeriodsForDay = timePeriods.Where(timePeriod => timePeriod.DaysOfWeek.Contains(dayOfWeek)).ToList();
            
            List<ValidationUnit> allUnits = new List<ValidationUnit>(_minutesInDay);
            for (int i = 0; i < _minutesInDay; i++)
            {
                allUnits.Add(new ValidationUnit());
            }

            foreach (var timePeriod in timePeriodsForDay)
            {
                int startTime = timePeriod.StartHour * 60 + timePeriod.StartMinute;
                int endTime = timePeriod.EndHour * 60 + timePeriod.EndMinute;
                
                for (var idx = startTime; idx < endTime; idx++)
                {
                    allUnits[idx].TryValidateFace(timePeriod.ClockFaceId);
                }
            }

            var cursorStart = -1;
            var cursorEnd = -1;

            for (int idx = 0; idx < allUnits.Count; idx++)
            {
                var unit = allUnits[idx];

                if (unit.IsValid && cursorEnd != -1)
                {
                    validationFailures.Add(new ValidationFailure()
                    {
                        ClockFaces = allUnits[idx - 1].ClockFacesPresent,
                        DayOfWeek = dayOfWeek,
                        EndHour = (int) Math.Floor(cursorEnd / 60.0),
                        EndMinute = cursorEnd % 60,
                        StartHour = (int) Math.Floor(cursorStart / 60.0),
                        StartMinute = cursorStart % 60,
                    });

                    cursorStart = -1;
                    cursorEnd = -1;
                }
                
                if (!allUnits[idx].IsValid)
                {
                    if (cursorStart == -1)
                    {
                        cursorStart = idx;
                    }

                    cursorEnd = idx;
                }
            }

            if (cursorStart != -1 && cursorEnd != -1)
            {
                validationFailures.Add(new ValidationFailure()
                {
                    ClockFaces = allUnits[_minutesInDay - 1].ClockFacesPresent,
                    DayOfWeek = dayOfWeek,
                    EndHour = (int) Math.Floor(cursorEnd / 60.0),
                    EndMinute = cursorEnd % 60,
                    StartHour = (int) Math.Floor(cursorStart / 60.0),
                    StartMinute = cursorStart % 60,
                });
            }
        }

        return new ValidationResponse()
        {
            ValidationFailures = validationFailures,
        };
    }
}