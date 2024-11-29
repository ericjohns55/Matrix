using System.Text;

namespace Matrix.Data.Models;

public class Timer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }
    
    private int TotalTicks => Hour * 3600 + Minute * 60 + Second;
    private int _currentTick = 0;

    public void Start()
    {
        _currentTick = TotalTicks;
    }

    public void Tick()
    {
        _currentTick--;
    }

    public bool HasEnded()
    {
        return _currentTick < 0;
    }

    public string GetFormattedTimer()
    {
        int hours = _currentTick / 3600;
        int minutes = _currentTick % 3600 / 60;
        int seconds = _currentTick % 60;
        
        var stringBuilder = new StringBuilder();
        if (hours > 0)
        {
            stringBuilder.Append($"{hours}:");
        }

        if (minutes > 0)
        {
            stringBuilder.Append($"{minutes:D2}:");
        }
        
        stringBuilder.Append($"{seconds:D2}");
        return stringBuilder.ToString();
    }
}