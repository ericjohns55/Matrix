using System.Text;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.TimeValidation;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.WebServices;
using Matrix.WebServices.Services;

namespace MatrixTests;

public class ClockFaceServiceTests : MatrixTestBase
{
    private MatrixContext _matrixContext;
    private ClockFaceService _clockFaceService;
    
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
        var activeFaces = await _clockFaceService.GetAllClockFaces();
        
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
        Assert.That(clockFace.TextLines.Count, Is.EqualTo(2));
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
        clockFace.Name = newName;
        
        // Act
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(updateResult.Name, Is.EqualTo(newName));
    }

    [Test]
    public async Task UpdateClockFace_EditedLines()
    {
        // Arrange
        SeedClockFaces(_matrixContext);
        
        var clockFace = (await _clockFaceService.GetClockFace(ActiveFaceId)).DeepCopy();
        foreach (var clockFaceTextLine in clockFace.TextLines)
        {
            clockFaceTextLine.Text = "UPDATED";
        }
        
        // Act
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.IsNotEmpty(updateResult.TextLines);
        
        foreach (var updateResultTextLine in updateResult.TextLines)
        {
            Assert.That(updateResultTextLine.Text, Is.EqualTo("UPDATED"));
        }
    }

    [Test]
    public async Task UpdateClockFace_NewLines()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var updatedText = "Changed Text";
        var clockFace = (await _clockFaceService.GetClockFace(ActiveFaceId)).DeepCopy();
        clockFace.TextLines = new List<TextLine>()
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
        
        // Act
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

        var clockFace = (await _clockFaceService.GetClockFace(ActiveFaceId)).DeepCopy();
        clockFace.TimePeriods = new List<TimePeriod>()
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
        var timePeriod = updateResult.TimePeriods.First();
        
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
    public async Task UpdateClockFace_UpdatedTimePeriods()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var clockFace = (await _clockFaceService.GetClockFace(ActiveFaceId)).DeepCopy();
        foreach (var originalTimePeriod in clockFace.TimePeriods)
        {
            originalTimePeriod.StartSecond = 2;
            originalTimePeriod.StartMinute = 2;
            originalTimePeriod.StartHour = 2;
            originalTimePeriod.EndSecond = 2;
            originalTimePeriod.EndMinute = 2;
            originalTimePeriod.EndHour = 2;
            originalTimePeriod.DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Monday };
        }
        
        // Act
        var updateResult = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        var timePeriod = updateResult.TimePeriods.First();
        
        // Assert
        Assert.NotNull(updateResult);
        Assert.NotNull(timePeriod);
        Assert.That(updateResult.Id, Is.EqualTo(ActiveFaceId));
        Assert.That(timePeriod.StartSecond, Is.EqualTo(2));
        Assert.That(timePeriod.StartMinute, Is.EqualTo(2));
        Assert.That(timePeriod.StartHour, Is.EqualTo(2));
        Assert.That(timePeriod.EndSecond, Is.EqualTo(2));
        Assert.That(timePeriod.EndMinute, Is.EqualTo(2));
        Assert.That(timePeriod.EndHour, Is.EqualTo(2));
        Assert.That(timePeriod.DaysOfWeek.Count, Is.EqualTo(1));
        Assert.True(timePeriod.DaysOfWeek.Contains(DayOfWeek.Monday));
    }

    [Test]
    public async Task UpdateClockFace_Everything()
    {
        // Arrange
        SeedClockFaces(_matrixContext);

        var clockFace = (await _clockFaceService.GetClockFace(ActiveFaceId)).DeepCopy();
        
        var newTextLines = new List<TextLine>();
        var keptLine = clockFace.TextLines.First();
        keptLine.Text = "UPDATED";
        newTextLines.Add(keptLine);
        
        newTextLines.Add(new TextLine()
        {
            Text = "New Text",
            Color = _matrixContext.MatrixColor.First(),
            Font = new MatrixFont()
            {
                FileLocation = string.Empty,
                Name = Guid.NewGuid().ToString()
            }
        });
        
        var newTimePeriods = new List<TimePeriod>();
        var keptPeriod = clockFace.TimePeriods.First();
        keptPeriod.StartSecond = 25;
        newTimePeriods.Add(keptPeriod);
        
        newTimePeriods.Add(new TimePeriod()
        {
            DaysOfWeek = new List<DayOfWeek>() { DayOfWeek.Monday },
            StartHour = 0,
            StartMinute = 0,
            EndHour = 12,
            EndMinute = 0,
        });
        
        clockFace.TextLines = newTextLines;
        clockFace.TimePeriods = newTimePeriods;
        clockFace.Name = "UPDATED";
        clockFace.IsTimerFace = true;
        
        // Act
        var updatedFace = await _clockFaceService.UpdateClockFace(ActiveFaceId, clockFace);
        
        // Assert
        Assert.NotNull(updatedFace);
        Assert.That(updatedFace.TextLines.Count, Is.EqualTo(2));
        Assert.True(updatedFace.TextLines.Any(line => line.Text == "UPDATED"));
        Assert.True(updatedFace.TextLines.Any(line => line.Text == "New Text"));
        Assert.That(updatedFace.TimePeriods.Count, Is.EqualTo(2));
        Assert.True(updatedFace.TimePeriods.Any(timePeriod => timePeriod.StartSecond == 25));
        Assert.True(updatedFace.TimePeriods.Any(timePeriod => timePeriod.EndHour == 12));
        Assert.True(updatedFace.Name == "UPDATED");
        Assert.True(updatedFace.IsTimerFace);
    }

    [Test]
    public void UpdateClockFace_NonExistent()
    {
        Assert.ThrowsAsync<MatrixEntityNotFoundException>(() => _clockFaceService.UpdateClockFace(-1, new ClockFace()));
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

    [Test]
    public async Task ValidateClockFaces_Single_Success()
    {
        // Arrange
        var clockFace = new ClockFace()
        {
            Name = "solo",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddAsync(clockFace);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.True(validation.SuccessfullyValidated);
    }

    [Test]
    public async Task ValidateClockFaces_Multiple_Success()
    {
        // Arrange
        var clockFace1 = new ClockFace()
        {
            Name = "face1",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 12,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        var clockFace2 = new ClockFace()
        {
            Name = "face2",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 12,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddRangeAsync(clockFace1, clockFace2);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.True(validation.SuccessfullyValidated);
    }

    [Test]
    public async Task ValidateClockFaces_MissingTime_Failure()
    {
        // Arrange
        var clockFace = new ClockFace()
        {
            Name = "solo",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 23,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddAsync(clockFace);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.False(validation.SuccessfullyValidated);
        Assert.That(validation.ValidationFailures?.Count, Is.EqualTo(7)); // one per day
        validation.ValidationFailures?.ForEach(failure =>
        {
            Assert.That(failure.StartHour, Is.EqualTo(23));
            Assert.That(failure.StartMinute, Is.EqualTo(0));
            Assert.That(failure.EndHour, Is.EqualTo(23));
            Assert.That(failure.EndMinute, Is.EqualTo(59));
            Assert.False(failure.TooManyFacesConfigured);
        });
        
        Console.WriteLine(FormatValidationResponse(validation));
    }

    [Test]
    public async Task ValidateClockFaces_MissingDay_Failure()
    {
        // Arrange
        var clockFace = new ClockFace()
        {
            Name = "solo",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday.Where(day => day != DayOfWeek.Sunday).ToList(),
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddAsync(clockFace);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.False(validation.SuccessfullyValidated);
        Assert.That(validation.ValidationFailures?.Count, Is.EqualTo(1)); // one per day
        validation.ValidationFailures?.ForEach(vf => Assert.False(vf.TooManyFacesConfigured));
        
        Console.WriteLine(FormatValidationResponse(validation));
    }

    [Test]
    public async Task ValidateClockFaces_Overlap_Failure()
    {
        // Arrange
        var clockFace1 = new ClockFace()
        {
            Name = "face1",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        var clockFace2 = new ClockFace()
        {
            Name = "face2",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 12,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddRangeAsync(clockFace1, clockFace2);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.False(validation.SuccessfullyValidated);
        Assert.That(validation.ValidationFailures?.Count, Is.EqualTo(7)); // one per day
        validation.ValidationFailures?.ForEach(failure =>
        {
            Assert.That(failure.StartHour, Is.EqualTo(12));
            Assert.That(failure.StartMinute, Is.EqualTo(0));
            Assert.That(failure.EndHour, Is.EqualTo(23));
            Assert.That(failure.EndMinute, Is.EqualTo(59));
            Assert.True(failure.TooManyFacesConfigured);
        });
        
        Console.WriteLine(FormatValidationResponse(validation));
    }

    [Test]
    public async Task ValidateClockFaces_All_Scenarios()
    {
        // Arrange
        var clockFace1 = new ClockFace()
        {
            Name = "face1",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = TimePeriod.Everyday,
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 23,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        var clockFace2 = new ClockFace()
        {
            Name = "face2",
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
                    StartHour = 23,
                    StartMinute = 0,
                    EndHour = 24,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        var clockFace3 = new ClockFace()
        {
            Name = "face3",
            TimePeriods = new List<TimePeriod>()
            {
                new TimePeriod()
                {
                    DaysOfWeek = new List<DayOfWeek>()
                    {
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    },
                    StartHour = 0,
                    StartMinute = 0,
                    EndHour = 12,
                    EndMinute = 0,
                }
            },
            TextLines = new List<TextLine>()
        };
        
        await _matrixContext.AddRangeAsync(clockFace1, clockFace2, clockFace3);
        await _matrixContext.SaveChangesAsync();
        
        // Act
        var validation = await _clockFaceService.ValidateClockFaceTimePeriods();
        
        // Assert
        Assert.False(validation.SuccessfullyValidated);
        Assert.That(validation.ValidationFailures?.Count, Is.EqualTo(4));
        Assert.That(validation.ValidationFailures.Count(f => f.DayOfWeek == DayOfWeek.Saturday), Is.EqualTo(2));
        Assert.That(validation.ValidationFailures.Count(f => f.DayOfWeek == DayOfWeek.Sunday), Is.EqualTo(2));
        Assert.That(validation.ValidationFailures.Count(f => f.TooManyFacesConfigured), Is.EqualTo(2));
        Assert.That(validation.ValidationFailures.Count(f => !f.TooManyFacesConfigured), Is.EqualTo(2));
        foreach (var validationFailure in validation.ValidationFailures.Where(v => v.TooManyFacesConfigured))
        {
            Assert.That(validationFailure.StartHour, Is.EqualTo(0));
            Assert.That(validationFailure.StartMinute, Is.EqualTo(0));
            Assert.That(validationFailure.EndHour, Is.EqualTo(11));
            Assert.That(validationFailure.EndMinute, Is.EqualTo(59));
            Assert.NotNull(validationFailure.ClockFaces);
            Assert.That(validationFailure.ClockFaces.Contains(1));
            Assert.That(validationFailure.ClockFaces.Contains(3));
        }
        
        Console.WriteLine(FormatValidationResponse(validation));
    }

    private string FormatValidationResponse(ValidationResponse validationResponse)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Overall Validation: {(validationResponse.SuccessfullyValidated ? "Valid" : "Invalid")}\n");

        if (validationResponse.ValidationFailures != null)
        {
            foreach (var failure in validationResponse.ValidationFailures)
            {
                var clockFaceNames = _matrixContext.ClockFace.ToDictionary(cf => cf.Id, cf => cf.Name);
                var faceNames = failure.ClockFaces?.Select(faceId => clockFaceNames[faceId]).ToList() ?? new List<string>();
        
                stringBuilder.AppendLine($"Day: {failure.DayOfWeek.ToString()}");
                stringBuilder.AppendLine($"Start: {failure.StartHour}:{failure.StartMinute:D2}");
                stringBuilder.AppendLine($"End: {failure.EndHour}:{failure.EndMinute:D2}");
                stringBuilder.AppendLine($"Clock faces present: {string.Join(", ", faceNames)}\n");
            }
        }

        return stringBuilder.ToString();
    }
}