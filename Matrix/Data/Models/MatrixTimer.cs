using System.Text;
using Matrix.Data.Types;

namespace Matrix.Data.Models;

public class MatrixTimer
{
    public int Hour { get; init; }
    public int Minute { get; init; }
    public int Second { get; init; }
    
    public TimerState State { get; private set; }
    
    public MatrixTimer(Timer timer)
    {
        Hour = timer.Hour;
        Minute = timer.Minute;
        Second = timer.Second;
        State = TimerState.Waiting;
    }
    
    private int TotalTicks => Hour * 3600 + Minute * 60 + Second;
    private int _currentTick = 0;

    public void Start(bool resetTicks = true)
    {
        if (resetTicks)
        {
            _currentTick = TotalTicks;
        }
        
        State = TimerState.Running;
    }

    public void Cancel()
    {
        State = TimerState.Cancelled;
        _currentTick = 0;
        
        ProgramState.RestorePreviousState(MatrixState.Timer);
    }

    public void Pause()
    {
        State = TimerState.Paused;
    }

    public void Tick(int blinkCount)
    {
        _currentTick--;
        
        if (State == TimerState.Running)
        {
            if (_currentTick == 0)
            {
                State = TimerState.Blinking;
            }
        }
        else if (State == TimerState.Blinking)
        {
            if (_currentTick == (blinkCount * -2 + 1))
            {
                State = TimerState.Complete;
            }
        }
    }

    public bool HasEnded()
    {
        return State == TimerState.Complete || State == TimerState.Cancelled;
    }

    public bool NeedsScreenUpdate()
    {
        return State == TimerState.Running || State == TimerState.Blinking;
    }

    public string GetFormattedTimer()
    {
        var stringBuilder = new StringBuilder();
        
        int currentTick = State == TimerState.Waiting ? TotalTicks : _currentTick;

        if (State == TimerState.Blinking || State == TimerState.Complete)
        {
            if (Math.Abs(_currentTick) % 2 == 0)
            {
                stringBuilder.Append("SCREEN ON");
            }
            else
            {
                stringBuilder.Append("BLINK OFF");
            }
        }
        else
        {
            int hours = currentTick / 3600;
            int minutes = currentTick % 3600 / 60;
            int seconds = currentTick % 60;
        
            if (hours > 0)
            {
                stringBuilder.Append($"{hours}:");
            }
        
            stringBuilder.Append($"{minutes:D2}:");
            stringBuilder.Append($"{seconds:D2}");
        }
        
        return stringBuilder.ToString();
    }
}