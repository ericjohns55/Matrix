using Matrix.Data.Utilities;

namespace Matrix.Data.Models;

public class ClockFace
{
    public int Id { get; init; }
    public string Name { get; set; }
    public List<TextLine> TextLines { get; set; }
    public List<TimePeriod> TimePeriods { get; set; }
    public bool Deleted { get; set; }

    public bool UpdatesEverySecond => TextLines?.Any(line => line.Text.Contains(VariableConstants.SecondVariable)) ?? false;
}