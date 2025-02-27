using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Models.TimeValidation;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class ClockFaceService
{
    private readonly MatrixContext _matrixContext;
    private readonly int _minutesInDay = 24 * 60;

    public ClockFaceService(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }
    
    public async Task<List<ClockFace>> GetAllClockFaces(
        SearchFilter filter = SearchFilter.Active,
        bool timerFace = false,
        bool render = false, 
        int scaleFactor = 1)
    {
        if (filter == SearchFilter.AllResults)
        {
            return await _matrixContext.ClockFace
                .Where(face => face.IsTimerFace == timerFace)
                .Include(face => face.TimePeriods)
                .Include(face => face.TextLines).ThenInclude(line => line.Color)
                .Include(face => face.TextLines).ThenInclude(line => line.Font)
                .Include(face => face.BackgroundImage)
                .ToListAsync();
        }
        
        var searchForDeleted = filter == SearchFilter.Deleted;

        var clockFaces = await _matrixContext.ClockFace.Where(face => face.Deleted == searchForDeleted && face.IsTimerFace == timerFace)
            .Include(face => face.TimePeriods)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .Include(face => face.BackgroundImage)
            .ToListAsync();

        if (render)
        {
            clockFaces.ForEach(face =>
            {
                var image = MatrixRenderer.RenderClockFace(face, scaleFactor);
                face.Base64Rendering = MatrixRenderer.ImageToBase64(image, true);
            });
        }

        return clockFaces;
    }

    public async Task<ClockFace> GetClockFace(int faceId, bool render = false, int scaleFactor = 1)
    {
        var clockFace = await _matrixContext.ClockFace.Where(face => face.Id == faceId)
            .Include(face => face.TimePeriods)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .Include(face => face.BackgroundImage)
            .FirstOrDefaultAsync();

        if (clockFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find clock face with id {faceId}");
        }

        if (render)
        {
            var image = MatrixRenderer.RenderClockFace(clockFace, scaleFactor);
            clockFace.Base64Rendering = MatrixRenderer.ImageToBase64(image, true);
        }

        return clockFace;
    }

    public async Task<ClockFace> GetTimerClockFace(int id)
    {
        var timerFace = await _matrixContext.ClockFace.Where(face => face.Id == id)
            .Include(face => face.TextLines).ThenInclude(line => line.Color)
            .Include(face => face.TextLines).ThenInclude(line => line.Font)
            .Include(face => face.BackgroundImage)
            .FirstOrDefaultAsync();

        if (timerFace == null)
        {
            throw new MatrixEntityNotFoundException($"Could not find timer clock face");
        }

        if (!timerFace.IsTimerFace)
        {
            throw new ClockFaceException(WebConstants.InvalidTimerFace);
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

        var updatedTextLines = new List<TextLine>();
        var newTextLines = updatedFace.TextLines.Where(textLine => textLine.Id == 0);
        var textLinesInBothFaces = updatedFace.TextLines
            .Where(textLine => originalFace.TextLines.Any(newLine => newLine.Id == textLine.Id));
        var textLinesNotInUpdatedFace = originalFace.TextLines
            .Where(textLine => updatedFace.TextLines.All(newLine => newLine.Id != textLine.Id));
        
        foreach (var newTextLine in newTextLines)
        {
            _matrixContext.Add(newTextLine);
            updatedTextLines.Add(newTextLine);
        };
        
        _matrixContext.RemoveRange(textLinesNotInUpdatedFace);

        foreach (var lineToUpdate in textLinesInBothFaces)
        {
            var oldLine = await _matrixContext.TextLine
                .SingleAsync(line => line.Id == lineToUpdate.Id && line.ClockFaceId == faceId);
            
            oldLine.Text = lineToUpdate.Text;
            oldLine.XLocation = lineToUpdate.XLocation;
            oldLine.YLocation = lineToUpdate.YLocation;
            oldLine.XPositioning = lineToUpdate.XPositioning;
            oldLine.YPositioning = lineToUpdate.YPositioning;
            oldLine.MatrixColorId = lineToUpdate.MatrixColorId;
            oldLine.MatrixFontId = lineToUpdate.MatrixFontId;
            oldLine.ClockFaceId = faceId;
            
            _matrixContext.TextLine.Update(oldLine);

            updatedTextLines.Add(oldLine);
        }

        var updatedTimePeriods = new List<TimePeriod>();
        var newTimePeriods = updatedFace.TimePeriods.Where(timePeriod => timePeriod.Id == 0);
        var timePeriodsInBothFaces = updatedFace.TimePeriods
            .Where(timePeriod => originalFace.TimePeriods.Any(newPeriod => newPeriod.Id == timePeriod.Id));
        var timePeriodsNotInUpdatedFace = originalFace.TimePeriods
            .Where(timePeriod => updatedFace.TimePeriods.All(newPeriod => newPeriod.Id != timePeriod.Id));

        foreach (var newTimePeriod in newTimePeriods)
        {
            _matrixContext.Add(newTimePeriod);
            updatedTimePeriods.Add(newTimePeriod);
        }
        
        _matrixContext.RemoveRange(timePeriodsNotInUpdatedFace);

        foreach (var periodToUpdate in timePeriodsInBothFaces)
        {
            var oldPeriod = await _matrixContext.TimePeriod.SingleAsync(period => period.Id == periodToUpdate.Id);
            
            oldPeriod.StartHour = periodToUpdate.StartHour;
            oldPeriod.StartMinute = periodToUpdate.StartMinute;
            oldPeriod.StartSecond = periodToUpdate.StartSecond;
            oldPeriod.EndHour = periodToUpdate.EndHour;
            oldPeriod.EndMinute = periodToUpdate.EndMinute;
            oldPeriod.EndSecond = periodToUpdate.EndSecond;
            oldPeriod.DaysOfWeek = new List<DayOfWeek>();
            oldPeriod.DaysOfWeek.AddRange(periodToUpdate.DaysOfWeek);
            oldPeriod.ClockFaceId = faceId;
            
            _matrixContext.TimePeriod.Update(oldPeriod);
            
            updatedTimePeriods.Add(oldPeriod);
        }
        
        originalFace.Name = updatedFace.Name;
        originalFace.IsTimerFace = updatedFace.IsTimerFace;
        originalFace.TextLines = updatedTextLines;
        originalFace.TimePeriods = updatedTimePeriods;
        originalFace.BackgroundImageId = updatedFace.BackgroundImageId;
        
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