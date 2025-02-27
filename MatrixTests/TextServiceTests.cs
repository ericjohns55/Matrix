using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.WebServices;
using Matrix.WebServices.Services;
using Microsoft.Extensions.Logging;

namespace MatrixTests;

public class TextServiceTests : MatrixTestBase
{
    private MatrixContext _matrixContext;
    private TextService _textService;

    [SetUp]
    public void Setup()
    {
        _matrixContext = CreateMatrixContext();
        _textService = new TextService(_matrixContext,
            new ImageService(_matrixContext, new Logger<ImageService>(new LoggerFactory())));
    }

    [TearDown]
    public void TearDown()
    {
        _matrixContext.Dispose();
    }
    
    [Test]
    public async Task GetPlainText()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        // Act
        var plainTextList = await _textService.GetSavedPlainText();

        // Assert
        Assert.That(plainTextList.Count, Is.EqualTo(1));
        var plainText = plainTextList.Single();
        Assert.That(plainText.Text, Is.EqualTo("Example plain text"));
        Assert.That(plainText.TextAlignment, Is.EqualTo(TextAlignment.Center));
        Assert.That(plainText.SplitByWord, Is.False);
    }

    [Test]
    public async Task GetPlainText_AfterAdd()
    {
        // Arrange
        SeedPlainText(_matrixContext);

        _matrixContext.Add(new PlainTextPayload()
        {
            Text = "Second plaintext",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        });
        
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var allPlainText = await _textService.GetSavedPlainText();
        
        // Assert
        Assert.That(allPlainText.Count, Is.EqualTo(2));
        Assert.True(allPlainText.Any(x => x.Text == "Example plain text"));
        Assert.True(allPlainText.Any(x => x.Text == "Second plaintext"));
    }

    [Test]
    public async Task GetPlainTextById()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        // Act
        var plainText = await _textService.GetPlainTextById(PlainTextId);
        
        // Assert
        Assert.That(plainText, Is.Not.Null);
        Assert.That(plainText.Id, Is.EqualTo(PlainTextId));
    }

    [Test]
    public void GetPlainTextById_Fail()
    {
        // Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.GetPlainTextById(-1));
    }

    [Test]
    public async Task AddPlainText_Success()
    {
        // Arrange
        var plainTextPayload = new PlainTextPayload()
        {
            Text = "Second plaintext",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act
        var plainText = await _textService.SavePlainText(plainTextPayload);

        // Assert
        Assert.That(plainText, Is.Not.Null);
        Assert.That(_matrixContext.SavedPlainText.Count, Is.EqualTo(1));
        Assert.That(_matrixContext.SavedPlainText.Single().Text, Is.EqualTo("Second plaintext"));
    }

    [Test]
    public void AddPlainText_NullPayload()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _textService.SavePlainText(null));
    }
    
    [Test]
    public async Task DeletePlainText_Success()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        // Act
        var response = await _textService.DeletePlainText(PlainTextId);
        
        // Assert
        Assert.IsEmpty(await _textService.GetSavedPlainText());
        Assert.That(response.Id, Is.EqualTo(PlainTextId));
    }

    [Test]
    public void DeletePlainText_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.DeletePlainText(-1));
    }

    [Test]
    public async Task UpdatePlainText_Success()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        var plainTextPayload = new PlainTextPayload()
        {
            Id = PlainTextId,
            Text = "Second plaintext",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act
        var response = await _textService.UpdatePlainText(PlainTextId, plainTextPayload);
        
        // Assert
        Assert.That(response.Id, Is.EqualTo(PlainTextId));
        Assert.That(response.Text, Is.EqualTo("Second plaintext"));
        Assert.That(response.TextAlignment, Is.EqualTo(TextAlignment.Center));
        Assert.That(response.SplitByWord, Is.False);
        Assert.That(response.Color!.Name, Is.EqualTo("temp"));
        Assert.That(response.Font!.Name, Is.EqualTo("temp"));
    }

    [Test]
    public async Task UpdatePlainText_OnlyName()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        var copiedPayload = (await _textService.GetPlainTextById(PlainTextId)).DeepCopy();
        copiedPayload.Text = "UPDATED";
        
        // Act
        var response = await _textService.UpdatePlainText(PlainTextId, copiedPayload);

        // Assert
        Assert.That(response.Id, Is.EqualTo(PlainTextId));
        Assert.That(response.Text, Is.EqualTo("UPDATED"));
    }

    [Test]
    public void UpdatePlainText_NullPayload()
    {
        // Arrange
        SeedPlainText(_matrixContext);
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _textService.UpdatePlainText(PlainTextId, null));
    }

    [Test]
    public void UpdatePlainText_NonExistent()
    {
        // Arrange
        var plainTextPayload = new PlainTextPayload()
        {
            Text = "Second plaintext",
            TextAlignment = TextAlignment.Center,
            SplitByWord = false,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act & Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.UpdatePlainText(-1, plainTextPayload));
    }
    
    [Test]
    public async Task GetScrollingText()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        // Act
        var scrollingTextList = await _textService.GetSavedScrollingText();

        // Assert
        Assert.That(scrollingTextList.Count, Is.EqualTo(1));
        var scrollingText = scrollingTextList.Single();
        Assert.That(scrollingText.Text, Is.EqualTo("Example scrolling text"));
    }

    [Test]
    public async Task GetScrollingText_AfterAdd()
    {
        // Arrange
        SeedScrollingText(_matrixContext);

        _matrixContext.Add(new ScrollingTextPayload()
        {
            Text = "Second ScrollingText",
            ScrollingDelay = 25,
            Iterations = 3,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        });
        
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var allScrollingText = await _textService.GetSavedScrollingText();
        
        // Assert
        Assert.That(allScrollingText.Count, Is.EqualTo(2));
        Assert.True(allScrollingText.Any(x => x.Text == "Example scrolling text"));
        Assert.True(allScrollingText.Any(x => x.Text == "Second ScrollingText"));
    }

    [Test]
    public async Task GetScrollingTextById()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        // Act
        var scrollingText = await _textService.GetScrollingTextById(ScrollingTextId);
        
        // Assert
        Assert.That(scrollingText, Is.Not.Null);
        Assert.That(scrollingText.Id, Is.EqualTo(ScrollingTextId));
    }

    [Test]
    public void GetScrollingTextById_Fail()
    {
        // Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.GetScrollingTextById(-1));
    }

    [Test]
    public async Task AddScrollingText_Success()
    {
        // Arrange
        var scrollingTextPayload = new ScrollingTextPayload()
        {
            Text = "Second ScrollingText",
            Iterations = 25,
            ScrollingDelay = 10,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act
        var scrollingText = await _textService.SaveScrollingText(scrollingTextPayload);

        // Assert
        Assert.That(scrollingText, Is.Not.Null);
        Assert.That(_matrixContext.SavedScrollingText.Count, Is.EqualTo(1));
        Assert.That(_matrixContext.SavedScrollingText.Single().Text, Is.EqualTo("Second ScrollingText"));
    }

    [Test]
    public void AddScrollingText_NullPayload()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _textService.SaveScrollingText(null));
    }
    
    [Test]
    public async Task DeleteScrollingText_Success()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        // Act
        var response = await _textService.DeleteScrollingText(ScrollingTextId);
        
        // Assert
        Assert.IsEmpty(await _textService.GetSavedScrollingText());
        Assert.That(response.Id, Is.EqualTo(ScrollingTextId));
    }

    [Test]
    public void DeleteScrollingText_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.DeleteScrollingText(-1));
    }

    [Test]
    public async Task UpdateScrollingText_Success()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        var scrollingTextPayload = new ScrollingTextPayload()
        {
            Id = ScrollingTextId,
            Text = "UPDATED",
            Iterations = 3,
            ScrollingDelay = 10,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act
        var response = await _textService.UpdateScrollingText(ScrollingTextId, scrollingTextPayload);
        
        // Assert
        Assert.That(response.Id, Is.EqualTo(ScrollingTextId));
        Assert.That(response.Text, Is.EqualTo("UPDATED"));
        Assert.That(response.Iterations, Is.EqualTo(3));
        Assert.That(response.ScrollingDelay, Is.EqualTo(10));
        Assert.That(response.Color!.Name, Is.EqualTo("temp"));
        Assert.That(response.Font!.Name, Is.EqualTo("temp"));
    }

    [Test]
    public async Task UpdateScrollingText_OnlyName()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        var copiedPayload = (await _textService.GetScrollingTextById(ScrollingTextId)).DeepCopy();
        copiedPayload.Text = "UPDATED";
        
        // Act
        var response = await _textService.UpdateScrollingText(ScrollingTextId, copiedPayload);

        // Assert
        Assert.That(response.Id, Is.EqualTo(ScrollingTextId));
        Assert.That(response.Text, Is.EqualTo("UPDATED"));
    }

    [Test]
    public void UpdateScrollingText_NullPayload()
    {
        // Arrange
        SeedScrollingText(_matrixContext);
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _textService.UpdateScrollingText(ScrollingTextId, null));
    }

    [Test]
    public void UpdateScrollingText_NonExistent()
    {
        // Arrange
        var scrollingTextPayload = new ScrollingTextPayload()
        {
            Id = ScrollingTextId,
            Text = "UPDATED",
            Iterations = 3,
            ScrollingDelay = 10,
            Color = new MatrixColor()
            {
                Name = "temp",
                Red = 255,
                Green = 255,
                Blue = 255
            },
            Font = new MatrixFont()
            {
                Name = "temp",
                FileLocation = Guid.NewGuid().ToString()
            }
        };
        
        // Act & Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _textService.UpdateScrollingText(-1, scrollingTextPayload));
    }
}