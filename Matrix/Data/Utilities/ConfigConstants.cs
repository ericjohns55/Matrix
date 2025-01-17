namespace Matrix.Data.Utilities;

public class ConfigConstants
{
    public static readonly string ServerUrl = "ServerUrl";
    public static readonly string DatabasePath = "DatabasePath";

    private static readonly string Seed = "Seed";
    public static readonly string RunSeedOnStart = $"{Seed}:RunOnStartup";
    public static readonly string SeedDrop = $"{Seed}:Drop";
    
    public static readonly string UpdateInterval = "UpdateInterval";
    public static readonly string TimerBlinkCount = "TimerBlinkCount";
    public static readonly string WeatherUrl = "WeatherUrl";
    public static readonly string FontsFolder = "FontsFolder";

    private static readonly string MatrixOptions = "MatrixOptions";
    public static readonly string HardwareMapping = $"{MatrixOptions}:HardwareMapping";
    public static readonly string Rows = $"{MatrixOptions}:Rows";
    public static readonly string Columns = $"{MatrixOptions}:Columns";
    public static readonly string ChainLength = $"{MatrixOptions}:ChainLength";
    public static readonly string Parallel = $"{MatrixOptions}:Parallel";
    public static readonly string Brightness = $"{MatrixOptions}:Brightness";
    public static readonly string LimitRefreshRateHz = $"{MatrixOptions}:LimitRefreshRateHz";
    public static readonly string DisableHardwarePulsing = $"{MatrixOptions}:DisableHardwarePulsing";
    public static readonly string GpioSlowdown = $"{MatrixOptions}:GpioSlowdown";

    private static readonly string Integrations = "Integrations";
    private static readonly string BuzzerSensor = $"{Integrations}:BuzzerSensor";
    public static readonly string AmbientSensorEnabled = $"{Integrations}:AmbientLightSensorEnabled";
    public static readonly string BuzzerSensorPin = $"{BuzzerSensor}:Pin";
    public static readonly string BuzzWithTimer = $"{BuzzerSensor}:BuzzWithTimer";
}