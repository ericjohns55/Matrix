namespace Matrix.Data.Models;

public class TimePeriod
{
    public int Id { get; init; }
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    public List<DayOfWeek> DaysOfWeek { get; init; }
}