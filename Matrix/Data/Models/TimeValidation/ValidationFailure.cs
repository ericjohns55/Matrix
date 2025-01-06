namespace Matrix.Data.Models.TimeValidation;

public class ValidationFailure
{
    public List<int> ClockFaces { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public int StartHour { get; init; }
    public int EndHour { get; init; }
    public int StartMinute { get; init; }
    public int EndMinute { get; init; }
}