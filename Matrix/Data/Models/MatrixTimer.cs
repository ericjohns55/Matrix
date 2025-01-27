using System.Text;
using Matrix.Data.Exceptions;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Data.Utilities;

namespace Matrix.Data.Models;

public class MatrixTimer
{
    public static readonly string ScreenOn = "SCREEN_ON";
    public static readonly string ScreenOff = "SCREEN_OFF";

    public static readonly string FinishedTimerText = "00:00";
    
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Second { get; private set; }

    private bool IsStopwatch { get; }

    public TimerState State { get; private set; }
    
    public MatrixTimer(Timer timer)
    {
        IsStopwatch = timer.IsStopwatch;
        
        if (!IsStopwatch)
        {
            Hour = timer.Hour;
            Minute = timer.Minute;
            Second = timer.Second;
        }
        
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
        if (IsStopwatch)
        {
            _currentTick++;

            Second++;

            if (Second > 60)
            {
                Second %= 60;
                Minute++;
            }

            if (Minute > 60)
            {
                Minute %= 60;
                Hour++;
            }
        }
        else
        {
            _currentTick--;
        }
        
        if (State == TimerState.Running)
        {
            if (_currentTick == 0 && !IsStopwatch)
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

    public void Reset()
    {
        if (!IsStopwatch)
        {
            throw new TimerException(WebConstants.TimerNotStopwatch);
        }

        Hour = 0;
        Minute = 0;
        Second = 0;
        
        _currentTick = 0;
    }

    public void Modify(TimerModification payload)
    {
        int newTickCount;
        
        if (payload.ModificationType == ModificationType.Subtract)
        {
            if (TotalTicks - payload.TickCount < 0)
            {
                throw new TimerException(WebConstants.InvalidModification);
            }
            
            newTickCount = TotalTicks - payload.TickCount;
        }
        else
        {
            newTickCount = TotalTicks +  payload.TickCount;
        }
        
        _currentTick = newTickCount;

        // normalize
        Hour = newTickCount / 3600;
        newTickCount %= 3600;
        
        Minute = newTickCount / 60;
        newTickCount %= 60;
        
        Second = newTickCount;
    }

    public string GetFormattedTimer()
    {
        int currentTick = State == TimerState.Waiting ? TotalTicks : _currentTick;

        if (currentTick < 0)
        {
            if (State == TimerState.Blinking || State == TimerState.Complete)
            {
                return Math.Abs(_currentTick % 2) == 1 ? ScreenOn : ScreenOff;
            }
        }

        var stringBuilder = new StringBuilder();
        
        int hours = currentTick / 3600;
        int minutes = currentTick % 3600 / 60;
        int seconds = currentTick % 60;
        
        if (hours > 0)
        {
            stringBuilder.Append($"{hours}:");
        }
        
        stringBuilder.Append($"{minutes:D2}:");
        stringBuilder.Append($"{seconds:D2}");
        
        return stringBuilder.ToString();
    }
}