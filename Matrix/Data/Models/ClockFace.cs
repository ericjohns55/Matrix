namespace Matrix.Data.Models;

public class ClockFace
{
    public int Id { get; init; }
    public string Name { get; init; }
    public List<TextLine> TextLines { get; init; }
    public List<TimePeriod> TimePeriods { get; init; }
}