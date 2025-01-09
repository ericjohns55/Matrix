using Matrix.Data;
using Matrix.Data.Utilities;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Utilities;
using Matrix.WebServices.Clients;
// using RPiRgbLEDMatrix;

using Color = System.Drawing.Color;

namespace Matrix.Display;

public class MatrixUpdater : IDisposable
{
    public static int MatrixWidth = 0;
    public static int MatrixHeight = 0;
    
    public MatrixClient MatrixClient;
    public WeatherClient? WeatherClient;
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
        if (!int.TryParse(matrixSettings[ConfigConstants.UpdateInterval], out _updateInterval))
        {
            throw new ConfigurationException("Could not parse UpdateInterval");
        }
        
        if (!int.TryParse(matrixSettings[ConfigConstants.TimerBlinkCount], out _timerBlinkCount))
        {
            throw new ConfigurationException("Could not parse UpdateInterval");
        }

        if (!string.IsNullOrWhiteSpace(matrixSettings[ConfigConstants.WeatherUrl]))
        {
            _weatherUrl = matrixSettings[ConfigConstants.WeatherUrl]!;
            WeatherClient = new WeatherClient(_weatherUrl);
        }

        var baseUrl = matrixSettings[ConfigConstants.ServerUrl];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _serverUrl = baseUrl;
            MatrixClient = new MatrixClient(_serverUrl);
        }

        if (!string.IsNullOrWhiteSpace(matrixSettings[ConfigConstants.FontsFolder]))
        {
            _fontsPath = matrixSettings[ConfigConstants.FontsFolder]!;
            
            var fontPath = Path.Combine(_fontsPath, "6x12.bdf");
            // _font = new RGBLedFont(fontPath);
        }

        try
        {
            MatrixWidth = matrixSettings.GetValue<int>(ConfigConstants.Columns);
            MatrixHeight = matrixSettings.GetValue<int>(ConfigConstants.Rows);
            
            // var options = new RGBLedMatrixOptions()
            // {
            //     HardwareMapping = matrixSettings[ConfigConstants.HardwareMapping],
            //     Rows = MatrixHeight,
            //     Cols =  MatrixWidth,
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

    public void HandleUpdateLoop(DateTime now)
    {
        if (ProgramState.State == MatrixState.Clock || ProgramState.State == MatrixState.Timer)
        {
            // update weather every 5 minutes; 10 seconds before next minutely update
            if ((now.Minute + 1) % 5 == 0 && now.Second == 50)
            {
                Console.WriteLine("Updating weather");
                ProgramState.Weather = WeatherClient?.GetWeather().WaitForCompletion() ?? WeatherModel.Empty;
            }

            // update clock face 10 seconds before each minute ends
            if (now.Second == 50)
            {
                var nextMinute = DateTime.Now.AddMinutes(1);
                
                ClockFace = MatrixClient.GetClockFaceForTime(new TimePayload()
                {
                    Hour = nextMinute.Hour,
                    Minute = nextMinute.Minute,
                    DayOfWeek = now.DayOfWeek
                }).WaitForCompletion();
            }
        }
        
        if (!ProgramState.NeedsUpdate(now, ClockFace))
        {
            return;
        }

        ProgramState.UpdateVariables();
        
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
    
    private void UpdateTimer()
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
                ProgramState.RestorePreviousState(MatrixState.Timer);
            }
            
            Console.WriteLine(timer.GetFormattedTimer());
        }
    }

    private void UpdateClock(DateTime time)
    {
        if (ClockFace != null)
        {
            var textLines = ClockFace.TextLines;
        }
        // var color = new RPiRgbLEDMatrix.Color(128, 0, 0);
        // _offscreenCanvas.DrawText(_font, 10, 10, color, DateTime.Now.ToString("HH:mm:ss"));
        
        // TODO: check for clock face changes
        Console.WriteLine(VariableUtility.ParseTime(time));
    }

    public void Dispose()
    {
        MatrixClient.Dispose();
        WeatherClient?.Dispose();
        // _matrix?.Dispose();
    }
}