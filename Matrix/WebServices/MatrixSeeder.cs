using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Utilities;

namespace Matrix.WebServices;

public class MatrixSeeder
{
    private readonly ILogger<MatrixSeeder> _logger;
    private readonly MatrixContext _matrixContext;

    public MatrixSeeder(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
        
        _logger = new LoggerFactory().CreateLogger<MatrixSeeder>();
    }

    public async Task Seed(bool drop)
    {
        _logger.LogInformation("Seeding matrix ...");
        if (drop)
        {
            _logger.LogInformation("Dropping data ...");
            
            _matrixContext.ClockFace.RemoveRange(_matrixContext.ClockFace);
            _matrixContext.TextLine.RemoveRange(_matrixContext.TextLine);
            _matrixContext.MatrixFont.RemoveRange(_matrixContext.MatrixFont);
            _matrixContext.MatrixColor.RemoveRange(_matrixContext.MatrixColor);
            _matrixContext.TimePeriod.RemoveRange(_matrixContext.TimePeriod);
            _matrixContext.Timer.RemoveRange(_matrixContext.Timer);
            
            await _matrixContext.SaveChangesAsync();
            _logger.LogInformation("Drop complete.");
        }
        
        _logger.LogInformation("Seeding matrix colors ...");
        
        var red = new MatrixColor() { Name = "red", Red = 128, Green = 0, Blue = 0, Deleted = false };
        var orange = new MatrixColor() { Name = "orange", Red = 253, Green = 88, Blue = 0, Deleted = false };
        var yellow = new MatrixColor() { Name = "yellow", Red = 255, Green = 228, Blue = 0, Deleted = false };
        var green = new MatrixColor() { Name = "green", Red = 0, Green = 160, Blue = 0, Deleted = false };
        var blue = new MatrixColor() { Name = "blue", Red = 0, Green = 64, Blue = 255, Deleted = false };
        var purple = new MatrixColor() { Name = "purple", Red = 128, Green = 0, Blue = 128, Deleted = false };
        var pink = new MatrixColor() { Name = "pink", Red = 255, Green = 0, Blue = 255, Deleted = false };
        var white = new MatrixColor() { Name = "white", Red = 255, Green = 255, Blue = 255, Deleted = false };
        var gray = new MatrixColor() { Name = "gray", Red = 128, Green = 128, Blue = 128, Deleted = false };
        var black = new MatrixColor() { Name = "black", Red = 0, Green = 0, Blue = 0, Deleted = false };
        var brown = new MatrixColor() { Name = "brown", Red = 101, Green = 67, Blue = 33, Deleted = false };
        
        await _matrixContext.AddRangeAsync(red, orange, yellow, green, blue, purple, pink, white, gray, black, brown);
        await _matrixContext.SaveChangesAsync();
        
        _logger.LogInformation("Seeding fonts...");
        
        var small = new MatrixFont() { Name = "5x8", FileLocation = "placeholder" };
        var medium = new MatrixFont() { Name = "6x9", FileLocation = "placeholder" };
        var large = new MatrixFont() { Name = "8x13", FileLocation = "placeholder" };
        var largeBold = new MatrixFont() { Name = "8x13B", FileLocation = "placeholder" };

        await _matrixContext.AddRangeAsync(small, medium, large, largeBold);
        await _matrixContext.SaveChangesAsync();
        
        _logger.LogInformation("Seeding clock faces ...");
        var dayFace = new ClockFace()
        {
            Name = "day",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday
                    },
                    StartHour = 7,
                    StartMinute = 0,
                    EndHour = 22,
                    EndMinute = 0,
                },
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    },
                    StartHour = 9,
                    StartMinute = 0,
                    EndHour = 23,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = VariableConstants.DayNameVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 8,
                    Color = purple,
                    Font = small
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.MonthNameVariable} {VariableConstants.MonthDayVariable}",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 16,
                    Color = purple,
                    Font = small
                },
                new TextLine()
                {
                    Text = VariableConstants.TimeFormattedVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 30,
                    Color = red,
                    Font = largeBold
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempVariable}F",
                    XPositioning = Positioning.XPositioning.CenterLeft,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = blue,
                    Font = medium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempFeelVariable}F",
                    XPositioning = Positioning.XPositioning.CenterRight,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = blue,
                    Font = medium
                },
                new TextLine()
                {
                    Text = VariableConstants.ForecastCurrentShortVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 51,
                    Color = blue,
                    Font = medium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.WindSpeedVariable} mph",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 60,
                    Color = blue,
                    Font = medium
                }
            }
        };
        
        var duskFace = new ClockFace()
        {
            Name = "dusk",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday
                    },
                    StartHour = 22,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                },
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    },
                    StartHour = 23,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                },
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    },
                    StartHour = 8,
                    StartMinute = 0,
                    EndHour = 9,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = VariableConstants.DayNameVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 8,
                    Color = red,
                    Font = small
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.MonthNameVariable} {VariableConstants.MonthDayVariable}",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 16,
                    Color = red,
                    Font = small
                },
                new TextLine()
                {
                    Text = VariableConstants.TimeFormattedVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 30,
                    Color = red,
                    Font = largeBold
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempVariable}F",
                    XPositioning = Positioning.XPositioning.CenterLeft,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = red,
                    Font = medium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempFeelVariable}F",
                    XPositioning = Positioning.XPositioning.CenterRight,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = red,
                    Font = medium
                },
                new TextLine()
                {
                    Text = VariableConstants.ForecastCurrentShortVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 51,
                    Color = red,
                    Font = medium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.WindSpeedVariable} mph",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 60,
                    Color = red,
                    Font = medium
                }
            }
        };
        
        var nightFace = new ClockFace()
        {
            Name = "night",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday
                    },
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 7,
                    EndMinute = 0,
                },
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    },
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 8,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = VariableConstants.DayNameVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Center,
                    Color = new MatrixColor()
                    {
                        Name = "Night Red",
                        Red = 32,
                        Green = 0,
                        Blue = 0
                    },
                    Font = largeBold
                }
            }
        };
        
        await _matrixContext.AddRangeAsync(dayFace, duskFace, nightFace);
        await _matrixContext.SaveChangesAsync();
    }
}