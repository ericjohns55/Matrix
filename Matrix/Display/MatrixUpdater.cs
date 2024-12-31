using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.WebServices;
// using RPiRgbLEDMatrix;

using Color = System.Drawing.Color;

namespace Matrix.Display;

public class MatrixUpdater : IDisposable
{
    private MatrixClient _client;
    public ClockFace? ClockFace { get; set; }

    private readonly int _updateInterval;
    private readonly int _timerBlinkCount;
    private readonly string _fontsPath;
    private readonly string _weatherUrl;
    private readonly string _serverUrl;

    // private readonly RGBLedMatrix _matrix;
    // private readonly RGBLedCanvas _offscreenCanvas;

    // private readonly RGBLedFont _font;

    public MatrixUpdater(IConfiguration matrixSettings)
    {
        ClockFace = new ClockFace();
        
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
            _serverUrl = baseUrl;
            _client = new MatrixClient(_serverUrl);
        }

        if (!string.IsNullOrWhiteSpace(matrixSettings[ConfigConstants.WeatherUrl]))
        {
            _weatherUrl = matrixSettings[ConfigConstants.WeatherUrl]!;
        }

        if (!string.IsNullOrWhiteSpace(matrixSettings[ConfigConstants.FontsFolder]))
        {
            _fontsPath = matrixSettings[ConfigConstants.FontsFolder]!;
            
            var fontPath = Path.Combine(_fontsPath, "6x12.bdf");
            // _font = new RGBLedFont(fontPath);
        }

        try
        {
            // var options = new RGBLedMatrixOptions()
            // {
            //     HardwareMapping = matrixSettings[ConfigConstants.HardwareMapping],
            //     Rows = matrixSettings.GetValue<int>(ConfigConstants.Rows),
            //     Cols =  matrixSettings.GetValue<int>(ConfigConstants.Columns),
            //     ChainLength = matrixSettings.GetValue<int>(ConfigConstants.ChainLength),
            //     Parallel = matrixSettings.GetValue<int>(ConfigConstants.Parallel),
            //     Brightness = matrixSettings.GetValue<int>(ConfigConstants.Brightness),
            //     LimitRefreshRateHz = matrixSettings.GetValue<int>(ConfigConstants.LimitRefreshRateHz),
            //     DisableHardwarePulsing = matrixSettings.GetValue<bool>(ConfigConstants.DisableHardwarePulsing),
            //     GpioSlowdown = matrixSettings.GetValue<int>(ConfigConstants.GpioSlowdown),
            // };
            //
            // _matrix = new RGBLedMatrix(options);
            // _offscreenCanvas = _matrix.CreateOffscreenCanvas();
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("Could not create Matrix using configured options\n" + ex.Message);
        }
    }

    public int GetUpdateInterval() => _updateInterval;

    public string GetServerUrl() => _serverUrl;
    
    public string GetWeatherUrl() => _weatherUrl;

    public void HandleUpdateLoop(DateTime now)
    {
        if (!ProgramState.NeedsUpdate(now, ClockFace))
        {
            return;
        }

        ProgramState.UpdateNextTick = false;
        
        // _offscreenCanvas.Clear();
        
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

        // _matrix.SwapOnVsync(_offscreenCanvas);
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
        // var color = new RPiRgbLEDMatrix.Color(128, 0, 0);
        // _offscreenCanvas.DrawText(_font, 10, 10, color, DateTime.Now.ToString("HH:mm:ss"));
        
        // TODO: check for clock face changes
        Console.WriteLine(VariableUtility.ParseTime(time));
    }

    public void Dispose()
    {
        _client?.Dispose();
        // _matrix?.Dispose();
    }
}