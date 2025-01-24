using Matrix.Data;
using Matrix.Data.Utilities;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Types;
using Matrix.Utilities;
using Matrix.WebServices.Clients;
using RPiRgbLEDMatrix;

namespace Matrix.Display;

public class MatrixUpdater : IDisposable
{
    public static ClockFace? OverridenClockFace { get; set; }

    public static int MatrixWidth = 0;
    public static int MatrixHeight = 0;

    public static int MatrixBrightness = 0;
    
    public MatrixClient MatrixClient;
    public WeatherClient? WeatherClient;
    public ClockFace? CurrentClockFace { get; set; }
    public ClockFace? TimerClockFace { get; set; }
    public string FontsPath { get; init; }

    private readonly int _updateInterval;
    private readonly int _timerBlinkCount;
    private readonly string _weatherUrl;
    private readonly string _serverUrl;

    private readonly RGBLedMatrix _matrix;
    private readonly RGBLedCanvas _offscreenCanvas;

    private DateTime _lastServerUpdateTime;

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
            FontsPath = matrixSettings[ConfigConstants.FontsFolder]!;
        }

        try
        {
            MatrixWidth = matrixSettings.GetValue<int>(ConfigConstants.Columns);
            MatrixHeight = matrixSettings.GetValue<int>(ConfigConstants.Rows);
            MatrixBrightness = matrixSettings.GetValue<int>(ConfigConstants.Brightness);
            
            var options = new RGBLedMatrixOptions()
            {
                HardwareMapping = matrixSettings[ConfigConstants.HardwareMapping],
                Rows = MatrixHeight,
                Cols =  MatrixWidth,
                ChainLength = matrixSettings.GetValue<int>(ConfigConstants.ChainLength),
                Parallel = matrixSettings.GetValue<int>(ConfigConstants.Parallel),
                Brightness = MatrixBrightness,
                LimitRefreshRateHz = matrixSettings.GetValue<int>(ConfigConstants.LimitRefreshRateHz),
                DisableHardwarePulsing = matrixSettings.GetValue<bool>(ConfigConstants.DisableHardwarePulsing),
                GpioSlowdown = matrixSettings.GetValue<int>(ConfigConstants.GpioSlowdown),
            };
            
            _matrix = new RGBLedMatrix(options);
            _offscreenCanvas = _matrix.CreateOffscreenCanvas();
        }
        catch (Exception ex)
        {
            throw new ConfigurationException("Could not create Matrix using configured options\n" + ex.Message);
        }
        
        _lastServerUpdateTime = DateTime.Now;
    }

    public void UpdateTimerFace(ClockFace timerFace)
    {
        TimerClockFace = timerFace;
    }

    public int GetUpdateInterval()
    {
        if (ProgramState.State == MatrixState.ScrollingText && ProgramState.ScrollingText != null)
        {
            return ProgramState.ScrollingText.ScrollingDelay;
        }
        
        return _updateInterval;
    }

    public string GetServerUrl() => _serverUrl;

    public void HandleUpdateLoop(DateTime now)
    {
        if (ProgramState.State == MatrixState.Clock || ProgramState.State == MatrixState.Timer)
        {
            var lastUpdate = now - _lastServerUpdateTime;

            // ensure that with scrolling text we do not spam requests
            if (lastUpdate.TotalSeconds > 5)
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
                
                    CurrentClockFace = MatrixClient.GetClockFaceForTime(new TimePayload()
                    {
                        Hour = nextMinute.Hour,
                        Minute = nextMinute.Minute,
                        DayOfWeek = now.DayOfWeek
                    }).WaitForCompletion();

                    // weather updates happen simultaneously with clock face updates
                    _lastServerUpdateTime = now;
                }
            }
        }
        
        if (!ProgramState.NeedsUpdate(now, CurrentClockFace))
        {
            return;
        }

        if (Convert.ToInt32(_matrix.Brightness) != MatrixBrightness)
        {
            _matrix.Brightness = Convert.ToByte(MatrixBrightness);
        }

        ProgramState.UpdateVariables();
        
        ProgramState.UpdateNextTick = false;
        
        _offscreenCanvas.Clear();

        if (ProgramState.State != MatrixState.Timer)
        {
            MatrixMain.Integrations.BuzzerSensor?.EnsureOff();
        }
        
        switch (ProgramState.State)
        {
            case MatrixState.Clock:
                UpdateClock();
                break;
            case MatrixState.Timer:
                UpdateTimer();
                break;
            case MatrixState.Text:
                UpdateText();
                break;
            case MatrixState.ScrollingText:
                UpdateScrollingText();
                break;
            case MatrixState.Image:
                DrawImage();
                break;
        }
        
        _matrix.SwapOnVsync(_offscreenCanvas);
    }
    
    private void UpdateClock()
    {
        var clockFaceForUpdate = ProgramState.OverrideClockFace ? OverridenClockFace : CurrentClockFace;
        
        if (clockFaceForUpdate != null)
        {
            DrawClockFace(clockFaceForUpdate);
        }
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

            string timerStatus = timer.GetFormattedTimer();

            if (timerStatus == MatrixTimer.ScreenOn)
            {
                if (MatrixMain.Integrations.BuzzWithTimer)
                {
                    MatrixMain.Integrations.BuzzerSensor?.Buzz(true);
                }
            }
            else if (timerStatus == MatrixTimer.ScreenOff)
            {
                if (MatrixMain.Integrations.BuzzWithTimer)
                {
                    MatrixMain.Integrations.BuzzerSensor?.Buzz(false);
                }
            }

            if (timerStatus != MatrixTimer.ScreenOff)
            {
                if (TimerClockFace != null)
                {
                    DrawClockFace(TimerClockFace);
                }
            }
        }
    }
    
    private void UpdateText()
    {
        if (ProgramState.PlainText != null)
        {
            var parsedLines = ProgramState.PlainText.ParseIntoTextLines();
            parsedLines.ForEach(parsedLine => DrawParsedTextLine(parsedLine));
        }
    }

    private void UpdateScrollingText()
    {
        if (ProgramState.ScrollingText != null)
        {
            if (ProgramState.ScrollingText.HandleUpdate())
            {
                DrawParsedTextLine(ProgramState.ScrollingText.GetParsedTextLine());
            }
            else
            {
                ProgramState.RestorePreviousState(MatrixState.Timer);
            }
        }
    }

    private void DrawImage()
    {
        if (ProgramState.Image != null)
        {
            for (int i = 0; i < MatrixHeight; i++)
            {
                for (int j = 0; j < MatrixWidth; j++)
                {
                    var pixel = ProgramState.Image[i, j];
                    _offscreenCanvas.SetPixel(i, j, 
                        new Color(pixel.R, pixel.G, pixel.B));
                }
            }
        }
    }

    private void DrawClockFace(ClockFace clockFace)
    {
        foreach (var textLine in clockFace.TextLines)
        {
            var parsedLine = TextLineParser.ParseTextLine(textLine, ProgramState.CurrentVariables);
            // Console.WriteLine(parsedLine.ParsedText);

            DrawParsedTextLine(parsedLine);
        }
    }

    private void DrawParsedTextLine(ParsedTextLine parsedLine)
    {
        _offscreenCanvas.DrawText(
            parsedLine.Font,
            parsedLine.XPosition,
            parsedLine.YPosition,
            parsedLine.Color,
            parsedLine.ParsedText);
    }

    public void Dispose()
    {
        MatrixClient.Dispose();
        WeatherClient?.Dispose();
        _matrix.Dispose();
    }
}