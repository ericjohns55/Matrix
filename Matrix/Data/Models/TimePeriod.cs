using System.ComponentModel.DataAnnotations.Schema;
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

    public int ClockFaceId { get; init; }

    public int StartHour { get; init; }
    public int EndHour { get; init; }
    
    public int StartMinute { get; init; }
    public int EndMinute { get; init; }
    
    public int StartSecond { get; init; }
    public int EndSecond { get; init; }
    
    public List<DayOfWeek> DaysOfWeek { get; init; }
}