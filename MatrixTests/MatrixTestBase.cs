using Matrix.Data.Models;
using Matrix.WebServices;
using Microsoft.EntityFrameworkCore;

namespace MatrixTests;

public class MatrixTestBase
{
    protected int ActiveColorId;
    protected int DeletedColorId;
    protected readonly string ActiveColorName = "Active Color";
    protected readonly string DeletedColorName = "Deleted Color";

    protected int ActiveFaceId;
    protected int DeletedFaceId;
    protected readonly string ActiveFaceName = "Active Face";
    protected readonly string DeletedFaceName = "Deleted Face";
    protected readonly string TextLineText = "Test";

    protected readonly List<TimePeriod> TimePeriods = new List<TimePeriod>()
    {
        new TimePeriod()
        {
            DaysOfWeek = TimePeriod.Everyday,
            StartHour = 0,
            StartMinute = 0,
            StartSecond = 0,
            EndHour = 23,
            EndMinute = 59,
            EndSecond = 59
        }
    };
    
    protected MatrixContext CreateMatrixContext(string? dbName = null)
    {
        var dbOptions = new DbContextOptionsBuilder<MatrixContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;
        
        return new MatrixContext(dbOptions);
    }

    protected void SeedTestingColors(MatrixContext context)
    {
        var activeColor = new MatrixColor()
        {
            Name = ActiveColorName,
            Red = 255,
            Green = 255,
            Blue = 255
        };

        var deletedColor = new MatrixColor()
        {
            Name = DeletedColorName,
            Red = 0,
            Green = 0,
            Blue = 0,
            Deleted = true
        };
        
        context.MatrixColor.Add(activeColor);
        context.MatrixColor.Add(deletedColor);
        context.SaveChanges();

        ActiveColorId = activeColor.Id;
        DeletedColorId = deletedColor.Id;
    }

    protected void SeedClockFaces(MatrixContext context)
    {
        var textColor = new MatrixColor()
        {
            Name = "Text Line Color",
            Red = 127,
            Green = 127,
            Blue = 127
        };
        
        context.MatrixColor.Add(textColor);
        context.SaveChanges();

        ClockFace activeFace = new ClockFace()
        {
            Name = ActiveFaceName,
            TimePeriods = TimePeriods,
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = TextLineText,
                    Color = textColor,
                    Font = new MatrixFont()
                    {
                        FileLocation = string.Empty,
                        Name = Guid.NewGuid().ToString(),
                    }
                }
            }
        };

        ClockFace deletedFace = new ClockFace()
        {
            Name = DeletedFaceName,
            Deleted = true,
            TimePeriods = TimePeriods,
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = TextLineText,
                    Color = textColor,
                    Font = new MatrixFont()
                    {
                        FileLocation = string.Empty,
                        Name = Guid.NewGuid().ToString(),
                    }
                }
            }
        };
        
        context.ClockFace.Add(activeFace);
        context.ClockFace.Add(deletedFace);
        
        context.SaveChanges();
        
        ActiveFaceId = activeFace.Id;
        DeletedFaceId = deletedFace.Id;
    }
}