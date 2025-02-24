using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Utilities;
using Matrix.WebServices;
using Matrix.WebServices.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private readonly string TextLineText1 = "Test1";
    private readonly string TextLineText2 = "Test2";

    protected int PlainTextId;
    protected int ScrollingTextId;
    protected int ImageId;
    
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
            TimePeriods = new List<TimePeriod>()
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
            },
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = TextLineText1,
                    Color = textColor,
                    Font = new MatrixFont()
                    {
                        FileLocation = string.Empty,
                        Name = Guid.NewGuid().ToString(),
                    }
                },
                new TextLine()
                {
                    Text = TextLineText2,
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
            TimePeriods = new List<TimePeriod>()
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
            },
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = TextLineText1,
                    Color = textColor,
                    Font = new MatrixFont()
                    {
                        FileLocation = string.Empty,
                        Name = Guid.NewGuid().ToString(),
                    }
                },
                new TextLine()
                {
                    Text = TextLineText2,
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

    protected void SeedPlainText(MatrixContext context)
    {
        var textColor = new MatrixColor()
        {
            Name = "Color",
            Red = 127,
            Green = 127,
            Blue = 127
        };
        
        context.MatrixColor.Add(textColor);

        var textFont = new MatrixFont()
        {
            Name = "Font",
            FileLocation = Guid.NewGuid().ToString(),
        };
        
        context.MatrixFont.Add(textFont);
        
        context.SaveChanges();
        
        var examplePlainText = new PlainTextPayload()
        {
            Text = "Example plain text",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            MatrixColorId = textColor.Id,
            MatrixFontId = textFont.Id
        };
        
        context.Add(examplePlainText);
        context.SaveChanges();
        
        PlainTextId = examplePlainText.Id;
    }

    protected void SeedScrollingText(MatrixContext context)
    {
        var textColor = new MatrixColor()
        {
            Name = "Color",
            Red = 127,
            Green = 127,
            Blue = 127
        };
        
        context.MatrixColor.Add(textColor);

        var textFont = new MatrixFont()
        {
            Name = "Font",
            FileLocation = Guid.NewGuid().ToString(),
        };
        
        context.MatrixFont.Add(textFont);
        
        context.SaveChanges();
        
        var exampleScrollingText = new ScrollingTextPayload()
        {
            Text = "Example scrolling text",
            Iterations = 3,
            ScrollingDelay = 10,
            MatrixColorId = textColor.Id,
            MatrixFontId = textFont.Id
        };
        
        context.Add(exampleScrollingText);
        context.SaveChanges();
        
        ScrollingTextId = exampleScrollingText.Id;
    }

    protected void SeedSavedImages(MatrixContext context)
    {
        var imageService = new ImageService(context, new Logger<ImageService>(new LoggerFactory()));

        imageService.SaveImage(new ImagePayload()
        {
            ImageName = "Example",
            Base64Image = MatrixSeeder.ExampleImageBase64
        }, Directory.GetCurrentDirectory()).WaitForCompletion();

        ImageId = context.SavedImage.SingleAsync().Id;

    }
}