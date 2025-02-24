using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices;
using Matrix.WebServices.Services;

namespace MatrixTests;

public class ColorServiceTests : MatrixTestBase
{
    private MatrixContext _matrixContext;
    private ColorService _colorService;
    
    [SetUp]
    public void Setup()
    {
        _matrixContext = CreateMatrixContext();
        _colorService = new ColorService(_matrixContext);
    }
    
    [TearDown]
    public void TearDown()
    {
        _matrixContext.Dispose();
    }

    [Test]
    public async Task GetMatrixColors_Active()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        var colors = await _colorService.GetMatrixColors(SearchFilter.Active);

        // Assert
        Assert.That(colors.Count, Is.EqualTo(1));
        Assert.That(colors[0].Name, Is.EqualTo(ActiveColorName));
        Assert.That(colors[0].Id, Is.EqualTo(ActiveColorId));
    }

    [Test]
    public async Task GetMatrixColors_Deleted()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        var colors = await _colorService.GetMatrixColors(SearchFilter.Deleted);

        // Assert
        Assert.That(colors.Count, Is.EqualTo(1));
        Assert.That(colors[0].Name, Is.EqualTo(DeletedColorName));
        Assert.That(colors[0].Id, Is.EqualTo(DeletedColorId));
    }

    [Test]
    public async Task GetMatrixColors_All()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        var colors = await _colorService.GetMatrixColors(SearchFilter.AllResults);
        
        // Assert
        Assert.That(colors.Count, Is.EqualTo(2));
        Assert.True(colors.Any(color => color.Deleted));
        Assert.True(colors.Any(color => !color.Deleted));
    }

    [Test]
    public async Task GetMatrixColor_Success()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        var activeColor = await _colorService.GetMatrixColor(ActiveColorId);
        var deletedColor = await _colorService.GetMatrixColor(DeletedColorId);
        
        // Assert
        Assert.NotNull(activeColor);
        Assert.That(activeColor.Id, Is.EqualTo(ActiveColorId));
        Assert.That(activeColor.Name, Is.EqualTo(ActiveColorName));
        
        Assert.NotNull(deletedColor);
        Assert.That(deletedColor.Id, Is.EqualTo(DeletedColorId));
        Assert.That(deletedColor.Name, Is.EqualTo(DeletedColorName));
    }

    [Test]
    public async Task GetMatrixColor_AfterAdd()
    {
        // Arrange
        SeedTestingColors(_matrixContext);

        var newColor = new MatrixColor()
        {
            Name = "New Color",
            Red = 0,
            Green = 3,
            Blue = 3
        };
        
        await _colorService.AddMatrixColor(newColor);
        
        // Act
        var addedColor = await _colorService.GetMatrixColor(newColor.Id);
        
        // Assert
        Assert.NotNull(addedColor);
        Assert.That(addedColor.Name, Is.EqualTo(newColor.Name));
    }

    [Test]
    public async Task GetMatrixColor_NonExistent()
    {
        // Arrange & Act
        var color = await _colorService.GetMatrixColor(-1);
        
        // Assert
        Assert.Null(color);
    }

    [Test]
    public async Task AddMatrixColor_Success()
    {
        // Arrange
        var newColor = new MatrixColor()
        {
            Name = "New Color",
            Red = 0,
            Green = 0,
            Blue = 0
        };
        
        await _colorService.AddMatrixColor(newColor);
        
        // Act
        var allColors = await _colorService.GetMatrixColors();
        var addedColor = await _colorService.GetMatrixColor(newColor.Id);
        
        // Assert
        Assert.That(allColors.Count, Is.EqualTo(1));
        Assert.NotNull(addedColor);
        Assert.That(addedColor.Name, Is.EqualTo(newColor.Name));
    }

    [Test]
    public void AddMatrixColor_NullPayload()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _colorService.AddMatrixColor(null));
    }

    [Test]
    public async Task UpdateMatrixColor_Success()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        var updatedColor = new MatrixColor()
        {
            Name = "Updated Name",
            Red = 127,
            Green = 127,
            Blue = 127
        };
        
        // Act
        var color = await _colorService.UpdateMatrixColor(ActiveColorId, updatedColor);
        
        // Assert
        Assert.NotNull(color);
        Assert.That(color.Id, Is.EqualTo(ActiveColorId));
        Assert.That(color.Name, Is.EqualTo(updatedColor.Name));
        Assert.That(color.Red, Is.EqualTo(127));
        Assert.That(color.Green, Is.EqualTo(127));
        Assert.That(color.Blue, Is.EqualTo(127));
    }

    [Test]
    public void UpdateMatrixColor_NonExistent()
    {
        // Arrange
        SeedTestingColors(_matrixContext);

        var newColor = new MatrixColor()
        {
            Name = "New",
            Red = 64,
            Green = 64,
            Blue = 64
        };
        
        // Act & Assert
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _colorService.UpdateMatrixColor(-1, newColor));
    }

    [Test]
    public void UpdateMatrixColor_NullPayload()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _colorService.UpdateMatrixColor(ActiveColorId, null));
    }

    [Test]
    public void DeleteMatrixColor_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _colorService.RemoveMatrixColor(-1));
    }

    [Test]
    public async Task DeleteMatrixColor_Success()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        int deletedColorId = await _colorService.RemoveMatrixColor(ActiveColorId);
        var activeColors = await _colorService.GetMatrixColors();
        
        // Assert
        Assert.That(deletedColorId, Is.EqualTo(ActiveColorId));
        Assert.IsEmpty(activeColors);
    }

    [Test]
    public void RestoreMatrixColor_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _colorService.RemoveMatrixColor(-1));
    }

    [Test]
    public async Task RestoreMatrixColor_Success()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        // Act
        var undeletedColor = await _colorService.RestoreMatrixColor(DeletedColorId);
        var allColors = await _colorService.GetMatrixColors();
        
        // Assert
        Assert.NotNull(undeletedColor);
        Assert.That(undeletedColor.Id, Is.EqualTo(DeletedColorId));
        Assert.That(undeletedColor.Name, Is.EqualTo(DeletedColorName));
        Assert.That(allColors.Count, Is.EqualTo(2));
    }
}