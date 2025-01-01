namespace Matrix.Data.Models;

public class ClockFace
{
    public int Id { get; init; }
    public string Name { get; set; }
    public List<TextLine> TextLines { get; set; }
    public List<TimePeriod> TimePeriods { get; set; }
    public bool Deleted { get; set; }

    // TODO: read text lines and check whether it is needed
    public bool UpdatesEverySecond => true;
}