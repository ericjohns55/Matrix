using System.Text.RegularExpressions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.WebServices.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.WebServices;

public class MatrixSeeder
{
    private readonly ILogger<MatrixSeeder> _logger;
    private readonly MatrixContext _matrixContext;
    private readonly string _dataFolderPath;

    public MatrixSeeder(MatrixContext matrixContext, string dataFolderPath)
    {
        _matrixContext = matrixContext;
        _dataFolderPath = dataFolderPath;
        
        _logger = new LoggerFactory().CreateLogger<MatrixSeeder>();
    }

    public async Task SeedFonts(string? fontsPath)
    {
        if (!string.IsNullOrWhiteSpace(fontsPath))
        {
            _matrixContext.MatrixFont.RemoveRange(_matrixContext.MatrixFont);
            await _matrixContext.SaveChangesAsync();
        
            var regex = new Regex("/[0-9]+[xX][0-9]*[BO]{0,1}");
        
            var fontsInFolder = Directory.GetFiles(fontsPath, "*.bdf", SearchOption.AllDirectories)
                .Where(filePath => regex.IsMatch(filePath))
                .Select(filePath => new MatrixFont()
                {
                    Name = filePath.Substring(fontsPath.Length + 1).Replace(".bdf", string.Empty),
                    FileLocation = filePath
                })
                .ToList();
            
            await _matrixContext.AddRangeAsync(fontsInFolder);
            await _matrixContext.SaveChangesAsync();
        }
    }

    public async Task Seed(bool drop, string? fontsPath = null)
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
            _matrixContext.SavedImage.RemoveRange(_matrixContext.SavedImage);
            _matrixContext.SavedPlainText.RemoveRange(_matrixContext.SavedPlainText);
            _matrixContext.SavedScrollingText.RemoveRange(_matrixContext.SavedScrollingText);
            
            await _matrixContext.SaveChangesAsync();
            _logger.LogInformation("Drop complete.");
        }
        
        _logger.LogInformation("Seeding matrix colors ...");
        
        var red = new MatrixColor() { Name = "Red", Red = 128, Green = 0, Blue = 0, Deleted = false };
        var orange = new MatrixColor() { Name = "Orange", Red = 253, Green = 88, Blue = 0, Deleted = false };
        var yellow = new MatrixColor() { Name = "Yellow", Red = 255, Green = 228, Blue = 0, Deleted = false };
        var green = new MatrixColor() { Name = "Green", Red = 0, Green = 160, Blue = 0, Deleted = false };
        var blue = new MatrixColor() { Name = "Blue", Red = 0, Green = 64, Blue = 255, Deleted = false };
        var purple = new MatrixColor() { Name = "Purple", Red = 128, Green = 0, Blue = 128, Deleted = false };
        var pink = new MatrixColor() { Name = "Pink", Red = 255, Green = 0, Blue = 255, Deleted = false };
        var white = new MatrixColor() { Name = "White", Red = 255, Green = 255, Blue = 255, Deleted = false };
        var gray = new MatrixColor() { Name = "Gray", Red = 128, Green = 128, Blue = 128, Deleted = false };
        var black = new MatrixColor() { Name = "Black", Red = 0, Green = 0, Blue = 0, Deleted = false };
        var brown = new MatrixColor() { Name = "Brown", Red = 101, Green = 67, Blue = 33, Deleted = false };
        
        await _matrixContext.AddRangeAsync(red, orange, yellow, green, blue, purple, pink, white, gray, black, brown);
        await _matrixContext.SaveChangesAsync();
        
        _logger.LogInformation("Seeding fonts...");
        await SeedFonts(fontsPath);
        
        var fontSmall = _matrixContext.MatrixFont.Single(font => font.Name == "5x8");
        var fontMedium = _matrixContext.MatrixFont.Single(font => font.Name == "6x9");
        var fontLarge = _matrixContext.MatrixFont.Single(font => font.Name == "8x13B");
        
        _logger.LogInformation("Seeding clock faces ...");
        var dayFace = new ClockFace()
        {
            Name = "Day",
            IsTimerFace = false,
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
                    Font = fontSmall
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.MonthNameVariable} {VariableConstants.MonthDayVariable}",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 16,
                    Color = purple,
                    Font = fontSmall
                },
                new TextLine()
                {
                    Text = VariableConstants.TimeFormattedVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 30,
                    Color = red,
                    Font = fontLarge
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempVariable}F",
                    XPositioning = Positioning.XPositioning.CenterLeft,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = blue,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempFeelVariable}F",
                    XPositioning = Positioning.XPositioning.CenterRight,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = blue,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = VariableConstants.ForecastCurrentShortVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 51,
                    Color = blue,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.WindSpeedVariable} mph",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 60,
                    Color = blue,
                    Font = fontMedium
                }
            }
        };
        
        var duskFace = new ClockFace()
        {
            Name = "Dusk",
            IsTimerFace = false,
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
                    Font = fontSmall
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.MonthNameVariable} {VariableConstants.MonthDayVariable}",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 16,
                    Color = red,
                    Font = fontSmall
                },
                new TextLine()
                {
                    Text = VariableConstants.TimeFormattedVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 30,
                    Color = red,
                    Font = fontLarge
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempVariable}F",
                    XPositioning = Positioning.XPositioning.CenterLeft,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = red,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.TempFeelVariable}F",
                    XPositioning = Positioning.XPositioning.CenterRight,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 42,
                    Color = red,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = VariableConstants.ForecastCurrentShortVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 51,
                    Color = red,
                    Font = fontMedium
                },
                new TextLine()
                {
                    Text = $"{VariableConstants.WindSpeedVariable} mph",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 60,
                    Color = red,
                    Font = fontMedium
                }
            }
        };
        
        var nightFace = new ClockFace()
        {
            Name = "Night",
            IsTimerFace = false,
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
                    Text = $"{VariableConstants.HourVariable}:{VariableConstants.MinuteVariable}{VariableConstants.AmPmVariable}",
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 35,
                    Color = new MatrixColor()
                    {
                        Name = "Night Red",
                        Red = 32,
                        Green = 0,
                        Blue = 0
                    },
                    Font = fontLarge
                }
            }
        };

        var timerFace = new ClockFace()
        {
            Name = "Timer Default",
            IsTimerFace = true,
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = VariableConstants.TimerFormattedVariable,
                    XPositioning = Positioning.XPositioning.Center,
                    YPositioning = Positioning.YPositioning.Absolute,
                    YLocation = 35,
                    Color = red,
                    Font = fontLarge
                }
            }
        };
        
        await _matrixContext.AddRangeAsync(dayFace, duskFace, nightFace, timerFace);
        await _matrixContext.SaveChangesAsync();

        _logger.LogInformation("Seeding sample scrolling text...");

        var exampleScrollingText = new ScrollingTextPayload()
        {
            Text = "Example scrolling text",
            Iterations = 3,
            ScrollingDelay = 10,
            MatrixColorId = red.Id,
            MatrixFontId = fontLarge.Id
        };

        await _matrixContext.AddAsync(exampleScrollingText);
        await _matrixContext.SaveChangesAsync();
        
        _logger.LogInformation("Seeding sample plain text...");

        var examplePlainText = new PlainTextPayload()
        {
            Text = "Example plain text",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            MatrixColorId = purple.Id,
            MatrixFontId = fontMedium.Id
        };
        
        await _matrixContext.AddAsync(examplePlainText);
        await _matrixContext.SaveChangesAsync();
        
        _logger.LogInformation("Seeding example image...");

        var imagesPath = Path.Combine(_dataFolderPath, ConfigConstants.SavedImagesFolder);

        if (!Directory.Exists(imagesPath))
        {
            _logger.LogInformation("Created saved_images directory");
            Directory.CreateDirectory(imagesPath);
        }
        else
        {
            _logger.LogInformation("Images directory already exists");
        }

        var imageService = new ImageService(_matrixContext, new Logger<ImageService>(new LoggerFactory()));
        await imageService.SaveImage(new ImagePayload()
        {
            ImageName = "Example Image",
            Base64Image = ExampleImageBase64
        });
        
        await _matrixContext.SaveChangesAsync();
    }
    
    public static readonly string ExampleImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABhGlDQ1BJQ0MgUHJvZmlsZQAAeJyVkT1Iw1AUhU9TtaIVBzuIOGSoDmIXFelYq1CECqFWaNXB5KV/0KQhSXFxFFwLDv4sVh1cnHV1cBUEwR8Qd8FJ0UVKvC8ptAgVvPB4H+fdc3nvPECol5lmdcUATbfNVCIuZrKrYuAVAvrhwwR6ZGYZc5KURMf6uqdOqrsIn4X/1YCasxjgE4ljzDBt4g3i2U3b4LxPHGJFWSU+J5406YLEj1xXPH7jXHBZ4DNDZjo1TxwiFgttrLQxK5oa8QxxWNV0mi9kPFY5b3HWylXWvCd/YTCnryxzndYoEljEEiSIUFBFCWXYiNCuk2IhRefxDv4R1y+RSyFXCYwcC6hAg+z6wf/gd7ZWfnrKmxSMA90vjvMxBgR2gUbNcb6PHadxAvifgSu95a/Ugegn6bWWFj4CBreBi+uWpuwBlzvA8JMhm7Ir+WkJ+TzwfkbflAWGboG+NS+35jlOH4A0ZZW8AQ4OgfECzV7v8O7e9tz+7Gnm9wM7InKQsx9JbQAAAAlwSFlzAAALEwAACxMBAJqcGAAAAM9lWElmSUkqAAgAAAAKAAABAwABAAAAQAAAAAEBAwABAAAAQAAAABIBAwABAAAAAQAAACgBAwABAAAAAgAAAAIBAwADAAAAhgAAABoBBQABAAAAjAAAABsBBQABAAAAlAAAADEBAgANAAAAnAAAADIBAgAUAAAAqQAAAGmHBAABAAAAvQAAAAAAAAAIAAgACAATCwAAAQAAABMLAAABAAAAR0lNUCAyLjEwLjM4ADIwMjU6MDI6MTAgMjE6NDA6NTQAAQABoAMAAQAAAAEAAAAAAAAAxGfJLQAADXppVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IlhNUCBDb3JlIDQuNC4wLUV4aXYyIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIgogICAgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIKICAgIHhtbG5zOkdJTVA9Imh0dHA6Ly93d3cuZ2ltcC5vcmcveG1wLyIKICAgIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIKICAgIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIgogICAgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIgogICB4bXBNTTpEb2N1bWVudElEPSJnaW1wOmRvY2lkOmdpbXA6ZDZiZmI4N2MtMzdmMy00YTMzLTkzMTktNGFmOGUyMDQwMTRkIgogICB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjczNzQzN2U4LWY1NDctNDQ4ZC1iZmY2LTJmMWI1YWJjNzhlYyIKICAgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjU1ODVlMGFiLTA0OGUtNDk0My04YmM2LTQ3MjM0YzQ1NzRkMiIKICAgR0lNUDpBUEk9IjIuMCIKICAgR0lNUDpQbGF0Zm9ybT0iTWFjIE9TIgogICBHSU1QOlRpbWVTdGFtcD0iMTczOTI0MTY1NTg2NzkxMCIKICAgR0lNUDpWZXJzaW9uPSIyLjEwLjM4IgogICBkYzpGb3JtYXQ9ImltYWdlL3BuZyIKICAgdGlmZjpPcmllbnRhdGlvbj0iMSIKICAgeG1wOkNyZWF0b3JUb29sPSJHSU1QIDIuMTAiCiAgIHhtcDpNZXRhZGF0YURhdGU9IjIwMjU6MDI6MTBUMjE6NDA6NTQtMDU6MDAiCiAgIHhtcDpNb2RpZnlEYXRlPSIyMDI1OjAyOjEwVDIxOjQwOjU0LTA1OjAwIj4KICAgPHhtcE1NOkhpc3Rvcnk+CiAgICA8cmRmOlNlcT4KICAgICA8cmRmOmxpCiAgICAgIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiCiAgICAgIHN0RXZ0OmNoYW5nZWQ9Ii8iCiAgICAgIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NjQ4ZmYwZDgtODc2Ni00MDEzLWFhZWMtYzY1Y2YwZDY1NmE4IgogICAgICBzdEV2dDpzb2Z0d2FyZUFnZW50PSJHaW1wIDIuMTAgKE1hYyBPUykiCiAgICAgIHN0RXZ0OndoZW49IjIwMjUtMDItMTBUMjE6NDA6NTUtMDU6MDAiLz4KICAgIDwvcmRmOlNlcT4KICAgPC94bXBNTTpIaXN0b3J5PgogIDwvcmRmOkRlc2NyaXB0aW9uPgogPC9yZGY6UkRGPgo8L3g6eG1wbWV0YT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgIAo8P3hwYWNrZXQgZW5kPSJ3Ij8+qRNAEAAAC4lJREFUeJztW19vFMkR7x4b27sgYXBeOGyjBO4hUpQo+QDJN490eU10Ue504qTDxwE23APYRsE7Noun01XV9adnemZn99ZSJK6Q2d3xbE/Xr/5XtTe3tr4I7jOmTfeZ068AuM+cfgXAfea0AgA+vfjyr0Pjbo7gmesNWqMB2Ij/fGR6snNAG5nsChZCcW8fzr5xIXxy6yfvpjuHbnb5kh7UAcNuZjxICwEAxjf8hvvL7t+S0H1cPrjv6iPXxM/V5J4+P/7cufdHAsE1a9aGYJivIhgH8tnD58mhY1Bm9Ut6/ggaBGDLb0XG/4oLP738MS6pyOL7QBsDZG7t/wavz0/euTt7f4o3hBvQBpI8Mf/KMRgTYd7R7+PnsSD0AgDM/zky//X5P+IyjbtO0pxOHqHUq/i8pj7HH/gMjN96uEdAhEBA3I/acPqf+PHarY+6mlBfHuOr0ngQigAw8/+OzH8Mc2QaqEbUifFq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";
}