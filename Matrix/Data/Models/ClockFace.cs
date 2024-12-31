namespace Matrix.Data.Models;

public class ClockFace
{
    public int Id { get; init; }
    public string Name { get; init; }
    public List<TextLine> TextLines { get; init; }
    public List<TimePeriod> TimePeriods { get; init; }
    public bool Deleted { get; set; }

    // TODO: read text lines and check whether it is needed
    public bool UpdatesEverySecond => true;
}