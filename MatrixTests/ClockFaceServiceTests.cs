using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices;
using Matrix.WebServices.Services;

namespace MatrixTests;

public class ClockFaceServiceTests : MatrixTestBase
{
    private MatrixContext _matrixContext;
    private IClockFaceService _clockFaceService;
    
    [SetUp]
    public void Setup()
    {
        _matrixContext = CreateMatrixContext();
        _clockFaceService = new ClockFaceService(_matrixContext);
    }
    
    [TearDown]
    public void TearDown()
    {
        _matrixContext.Dispose();
    }

    [Test]
    public async Task GetAllClockFaces_Active()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var activeFaces = await _clockFaceService.GetAllClockFaces(SearchFilter.Active);
        
        // Assert
        Assert.That(activeFaces.Count, Is.EqualTo(1));
        Assert.That(activeFaces.FirstOrDefault()!.Name, Is.EqualTo(ActiveFaceName));
    }

    [Test]
    public async Task GetAllClockFaces_Inactive()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var deletedFaces = await _clockFaceService.GetAllClockFaces(SearchFilter.Deleted);
        
        // Assert
        Assert.That(deletedFaces.Count, Is.EqualTo(1));
        Assert.That(deletedFaces.FirstOrDefault()!.Name, Is.EqualTo(DeletedFaceName));
    }

    [Test]
    public async Task GetAllClockFaces_All()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var allFaces = await _clockFaceService.GetAllClockFaces(SearchFilter.AllResults);
        
        // Assert
        Assert.That(allFaces.Count, Is.EqualTo(2));
        Assert.True(allFaces.Any(f => f.Name == ActiveFaceName));
        Assert.True(allFaces.Any(f => f.Id == ActiveFaceId));
        Assert.True(allFaces.Any(f => f.Name == DeletedFaceName));
        Assert.True(allFaces.Any(f => f.Id == DeletedFaceId));
    }

    [Test]
    public async Task GetClockFace_Success()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var clockFace = await _clockFaceService.GetClockFace(ActiveFaceId);
        
        // Assert
        Assert.NotNull(clockFace);
        Assert.That(clockFace.Name, Is.EqualTo(ActiveFaceName));
        Assert.That(clockFace.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(clockFace.TextLines.Count, Is.EqualTo(1));
        Assert.That(clockFace.TimePeriods.Count, Is.EqualTo(1));
        Assert.False(clockFace.Deleted);
    }

    [Test]
    public void GetClockFace_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _clockFaceService.GetClockFace(-1));
    }

    [Test]
    public async Task UpdateClockFace_Success()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var newName = "Updated Name";
        var clockFace = await _clockFaceService.GetClockFace(ActiveFaceId);
        clockFace!.Name = newName;
        
        // Act
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(updateResult.Name, Is.EqualTo(newName));
    }

    [Test]
    public async Task UpdateClockFace_NewLines()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var updatedText = "Changed Text";
        var clockFace = await _clockFaceService.GetClockFace(ActiveFaceId);
        clockFace!.TextLines = new List<TextLine>()
        {
            new TextLine()
            {
                Text = updatedText,
                Color = _matrixContext.MatrixColor.First(),
                Font = new MatrixFont()
                {
                    FileLocation = string.Empty,
                    Name = Guid.NewGuid().ToString()
                }
            }
        };
        
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(updateResult.TextLines.Count, Is.EqualTo(1));
        Assert.That(updateResult.TextLines[0].Text, Is.EqualTo(updatedText));
    }

    [Test]
    public async Task UpdateClockFace_NewTimePeriods()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var clockFace = await _clockFaceService.GetClockFace(ActiveFaceId);
        clockFace!.TimePeriods = new List<TimePeriod>()
        {
            new TimePeriod()
            {
                StartSecond = 1,
                StartMinute = 1,
                StartHour = 1,
                EndSecond = 1,
                EndMinute = 1,
                EndHour = 1,
                DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Monday }
            }
        };
        
        // Act
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        var timePeriod = updateResult?.TimePeriods.First();
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.NotNull(timePeriod);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(updateResult.TimePeriods.Count, Is.EqualTo(1));
        Assert.That(timePeriod.StartSecond, Is.EqualTo(1));
        Assert.That(timePeriod.StartMinute, Is.EqualTo(1));
        Assert.That(timePeriod.StartHour, Is.EqualTo(1));
        Assert.That(timePeriod.EndSecond, Is.EqualTo(1));
        Assert.That(timePeriod.EndMinute, Is.EqualTo(1));
        Assert.That(timePeriod.EndHour, Is.EqualTo(1));
        Assert.That(timePeriod.DaysOfWeek.Count, Is.EqualTo(1));
        Assert.That(timePeriod.DaysOfWeek.First(), Is.EqualTo(DayOfWeek.Monday));
    }

    [Test]
    public void UpdateClockFace_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _clockFaceService.UpdateClockFace(-1, null));
    }

    [Test]
    public async Task AddClockFace_Success()
    {
        // Arrange
        SeedTestingColors(_matrixContext);
        
        var newFace = new ClockFace()
        {
            Name = "New Face",
            TextLines = new List<TextLine>()
            {
                new TextLine()
                {
                    Text = "New Text",
                    Color = _matrixContext.MatrixColor.First(),
                    Font = new MatrixFont()
                    {
                        FileLocation = string.Empty,
                        Name = Guid.NewGuid().ToString()
                    }
                }
            },
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    EndHour = 11,
                    EndMinute = 59,
                    EndSecond = 59
                }
            }
        };
        
        // Act
        var insertedFace = await _clockFaceService.AddClockFace(newFace);
        
        // Assert
        Assert.NotNull(insertedFace);
        Assert.That(insertedFace.Name, Is.EqualTo(newFace.Name));
        Assert.That(insertedFace.TextLines.Count, Is.EqualTo(1));
        Assert.That(insertedFace.TimePeriods.Count, Is.EqualTo(1));
    }

    [Test]
    public void AddClockFace_NullPayload()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _clockFaceService.AddClockFace(null));
    }

    [Test]
    public async Task RemoveClockFace_Success()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var removedId = await _clockFaceService.RemoveClockFace(ActiveFaceId);
        var allFaces = await _clockFaceService.GetAllClockFaces(SearchFilter.AllResults);
        
        // Assert
        Assert.That(removedId, Is.EqualTo(ActiveFaceId));
        Assert.That(allFaces.Count(f => f.Deleted), Is.EqualTo(2));
        Assert.That(allFaces.Count(f => !f.Deleted), Is.EqualTo(0));
    }

    [Test]
    public void RemoveClockFace_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _clockFaceService.RemoveClockFace(-1));
    }

    [Test]
    public async Task RestoreClockFace_Success()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        // Act
        var restoredFace = await _clockFaceService.RestoreClockFace(DeletedFaceId);
        var allFaces = await _clockFaceService.GetAllClockFaces(SearchFilter.AllResults);
        
        // Assert
        Assert.That(restoredFace.Id, Is.EqualTo(DeletedFaceId));
        Assert.That(allFaces.Count(f => f.Deleted), Is.EqualTo(0));
        Assert.That(allFaces.Count(f => !f.Deleted), Is.EqualTo(2));
    }

    [Test]
    public void RestoreClockFace_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _clockFaceService.RestoreClockFace(-1));
    }
}