using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class TimePeriod
{
    public static List<DayOfWeek> Everyday = new List<DayOfWeek>()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };
    
    public int Id { get; init; }

    public ClockFace ClockFace { get; init; }

    public int ClockFaceId { get; set; }

    public int StartHour { get; set; }
    public int EndHour { get; set; }
    
    public int StartMinute { get; set; }
    public int EndMinute { get; set; }
    
    public int StartSecond { get; set; }
    public int EndSecond { get; set; }
    
    public List<DayOfWeek> DaysOfWeek { get; set; }
}