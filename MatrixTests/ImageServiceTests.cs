using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Utilities;
using Matrix.WebServices;
using Matrix.WebServices.Services;
using Microsoft.Extensions.Logging;

namespace MatrixTests;

public class ImageServiceTests : MatrixTestBase
{
    private MatrixContext _matrixContext;
    private ImageService _imageService;

    [SetUp]
    public void Setup()
    {
        _matrixContext = CreateMatrixContext();
        _imageService = new ImageService(_matrixContext, new Logger<ImageService>(new LoggerFactory()));
    }

    [TearDown]
    public void TearDown()
    {
        var allImages = _matrixContext.SavedImage.ToList();

        foreach (var image in allImages)
        {
            _imageService.DeleteImage(image.Id).WaitForCompletion();
        }
        
        _matrixContext.Dispose();
    }

    [Test]
    [NonParallelizable]
    public async Task GetImages()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
        
        // Act
        var allImages = await _imageService.GetSavedImages(0, false, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.That(allImages.Count, Is.EqualTo(1));
        Assert.That(allImages[0].Name, Is.EqualTo("Example"));
        
    }

    [Test]
    [NonParallelizable]
    public async Task GetImage_After_Add()
    {
        // Arrange
        SeedSavedImages(_matrixContext);

        var newImage = new ImagePayload()
        {
            ImageName = "TestImage",
            Base64Image = MatrixSeeder.ExampleImageBase64
        };
        
        await _imageService.SaveImage(newImage, Directory.GetCurrentDirectory());
        
        // Act
        var allImages = await _imageService.GetSavedImages(0, false, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.That(allImages.Count, Is.EqualTo(2));
        Assert.True(allImages.Any(image => image.Name == "Example"));
        Assert.True(allImages.Any(image => image.Name == "TestImage"));
    }

    [Test]
    [NonParallelizable]
    public async Task GetImageById_NoRendering()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
         
        // Act
        var image = await _imageService.GetImageById(ImageId, false, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(image);
        Assert.That(image.Name, Is.EqualTo("Example"));
    }

    [Test]
    [NonParallelizable]
    public async Task GetImageById_WithRendering()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
         
        // Act
        var image = await _imageService.GetImageById(ImageId, true, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(image);
        Assert.That(image.Name, Is.EqualTo("Example"));
        Assert.NotNull(image.Image);

        await Task.Delay(1000);
    }

    [Test]
    [NonParallelizable]
    public void GetImageById_Fail()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _imageService.GetImageById(-1));
    }

    [Test]
    [NonParallelizable]
    public async Task AddImage_Success()
    {
        // Arrange
        var newImage = new ImagePayload()
        {
            ImageName = "TestImage",
            Base64Image = MatrixSeeder.ExampleImageBase64
        };
        
        // Act
        var result = await _imageService.SaveImage(newImage, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(result);
        Assert.That(result.Name, Is.EqualTo("TestImage"));
    }

    [Test]
    [NonParallelizable]
    public void SaveImage_Null_Payload()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _imageService.SaveImage(null));
    }
    
    [Test]
    [NonParallelizable]
    public async Task DeleteImage_Success()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
        
        // Act
        var result = await _imageService.DeleteImage(ImageId, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(result);
        Assert.That(result.Name, Is.EqualTo("Example"));
        Assert.IsEmpty(_matrixContext.SavedImage);
    }

    [Test]
    [NonParallelizable]
    public void DeleteImage_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _imageService.DeleteImage(-1, Directory.GetCurrentDirectory()));
    }

    [Test]
    [NonParallelizable]
    public async Task UpdateImage_Rendering()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
        
        var originalName = _matrixContext.SavedImage.First().Name;
        var originalFileId = _matrixContext.SavedImage.First().FileName;
        var newImage = new ImagePayload()
        {
            Base64Image = MatrixSeeder.ExampleImageBase64
        };
        
        // Act
        var result = await _imageService.UpdateImage(ImageId, newImage, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(result);
        Assert.That(result.Name, Is.EqualTo(originalName));
        Assert.That(result.FileName, !Is.EqualTo(originalFileId));
    }

    [Test]
    [NonParallelizable]
    public async Task UpdatePlainText_OnlyName()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
        
        var originalName = _matrixContext.SavedImage.First().Name;
        var originalFileId = _matrixContext.SavedImage.First().FileName;
        var newImage = new ImagePayload()
        {
            ImageName = "UPDATED"
        };
        
        // Act
        var result = await _imageService.UpdateImage(ImageId, newImage, Directory.GetCurrentDirectory());
        
        // Assert
        Assert.NotNull(result);
        Assert.That(result.Name, !Is.EqualTo(originalName));
        Assert.That(result.FileName, Is.EqualTo(originalFileId));
    }

    [Test]
    [NonParallelizable]
    public void UpdatePlainText_NullPayload()
    {
        // Arrange
        SeedSavedImages(_matrixContext);
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _imageService.UpdateImage(ImageId, null));
    }

    [Test]
    [NonParallelizable]
    public void UpdatePlainText_NonExistent()
    {
        // Arrange
        var newImage = new ImagePayload()
        {
            ImageName = "New",
            Base64Image = "N/A"
        };
        
        // Act & Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _imageService.UpdateImage(-1, newImage));
    }
}