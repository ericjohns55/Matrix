namespace Matrix.Data.Models;

public class Timer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    public int TimerFaceId { get; set; }
    public bool IsStopwatch { get; set; }
    public bool Deleted { get; init; }
}