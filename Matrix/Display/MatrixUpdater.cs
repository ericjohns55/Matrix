using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices;

namespace Matrix.Display;

public class MatrixUpdater : IDisposable
{
    private MatrixClient _client;
    public ClockFace? ClockFace { get; set; }

    private readonly int _updateInterval;
    private readonly int _timerBlinkCount;

    public MatrixUpdater(IConfiguration matrixSettings)
    {
        if (!int.TryParse(matrixSettings[ConfigConstants.UpdateInterval], out _updateInterval))
        {
            throw new ConfigurationException("Could not parse UpdateInterval");
        }
        
        if (!int.TryParse(matrixSettings[ConfigConstants.TimerBlinkCount], out _timerBlinkCount))
        {
            throw new ConfigurationException("Could not parse UpdateInterval");
        }

        var baseUrl = matrixSettings[ConfigConstants.ServerUrl];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _client = new MatrixClient(baseUrl);
        }
    }

    public int GetUpdateInterval() => _updateInterval;

    public void HandleUpdateLoop(DateTime now)
    {
        if (!ProgramState.NeedsUpdate(now, ClockFace))
        {
            return;
        }

        ProgramState.UpdateNextTick = false;
        
        switch (ProgramState.State)
        {
            case MatrixState.Clock:
                UpdateClock(now);
                break;
            case MatrixState.Timer:
                UpdateTimer();
                break;
            case MatrixState.Canvas:
                break;
            case MatrixState.Text:
                break;
            case MatrixState.ScrollingText:
                break;
            case MatrixState.Image:
                break;
        }
    }
    
    public void UpdateTimer()
    {
        var timer = ProgramState.Timer;

        if (timer != null)
        {
            if (timer.State == TimerState.Running || timer.State == TimerState.Blinking)
            {
                timer.Tick(_timerBlinkCount);
            }

            if (timer.HasEnded())
            {
                ProgramState.State = ProgramState.PreviousState;
                ProgramState.PreviousState = MatrixState.Timer;
                ProgramState.UpdateNextTick = true;
            }
            
            Console.WriteLine(timer.GetFormattedTimer());
        }
    }

    public void UpdateClock(DateTime time)
    {
        // TODO: check for clock face changes
        Console.WriteLine(VariableUtility.ParseTime(time));
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}