using System.ComponentModel.DataAnnotations.Schema;
using Matrix.Data.Utilities;

namespace Matrix.Data.Models;

public class ClockFace
{
    public int Id { get; init; }
    public string Name { get; set; }
    public List<TextLine> TextLines { get; set; }
    public List<TimePeriod> TimePeriods { get; set; }
    public bool IsTimerFace { get; set; }
    public bool Deleted { get; set; }
    
    public int? BackgroundImageId { get; set; }
    public SavedImage? BackgroundImage { get; set; }
    
    [NotMapped]
    public string? Base64Rendering { get; set; } = null;

    public bool UpdatesEverySecond => TextLines?.Any(line => line.Text.Contains(VariableConstants.SecondVariable)) ?? false;
}