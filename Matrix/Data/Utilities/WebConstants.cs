namespace Matrix.Data.Utilities;

public class WebConstants
{
    public static readonly string LightSensorSource = "LightSensor";
    
    public static readonly string TimerNull = "A timer does not exist.";
    public static readonly string ClockFaceNull = "Provided clock face was null.";
    public static readonly string TimePayloadNull = "Provided time payload was null.";
    public static readonly string MultipleClockFaces = "Multiple clock faces were found for the given time";
    public static readonly string InvalidTimerFace = "Provided clock face ID does not yield a timer result";
    public static readonly string ClockFaceIsTimer = "Provided clock face ID unexpectedly yielded a timer face";
    public static readonly string AmbientSensorDisabled = "The ambient sensor is disabled";
    public static readonly string ColorNotFound = "Could not load provided color";
    public static readonly string FontNotFound = "Could not load provided font";
}