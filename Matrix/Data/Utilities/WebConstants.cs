namespace Matrix.Data.Utilities;

public class WebConstants
{
    public static readonly string LightSensorSource = "LightSensor";
    
    public static readonly string TimerNull = "A timer does not exist.";
    public static readonly string ClockFaceNull = "Provided clock face was null.";
    public static readonly string TimePayloadNull = "Provided time payload was null.";
    public static readonly string MultipleClockFaces = "Multiple clock faces were found for the given time";
    public static readonly string InvalidTimerFace = "Provided clock face ID does not yield a timer result";
    public static readonly string InvalidModification = "This modification would end your timer";
    public static readonly string TimerNotStopwatch = "The timer is not a stopwatch";
    public static readonly string ClockFaceIsTimer = "Provided clock face ID unexpectedly yielded a timer face";
    public static readonly string AmbientSensorDisabled = "The ambient sensor is disabled";
    public static readonly string ColorNotFound = "Could not load provided color";
    public static readonly string FontNotFound = "Could not load provided font";
    public static readonly string MissingImage = "Image could not be found";
    public static readonly string InvalidImageSize = "Provided image was not matrix size";
    public static readonly string TextNotFound = "Could not find text object";
    public static readonly string MissingFontInformation = "Could not find font information";
    public static readonly string PlainTextNotFound = "Could not find plain text";
    public static readonly string PlainTextNotValid = "The provided plain text does not reference valid matrix entities";
    public static readonly string ScrollingTextNotFound = "Could not find scrolling text";
    public static readonly string ScrollingTextNotValid = "The provided scrolling text does not reference valid matrix entities";
    public static readonly string MissingImageInformation = "Could not find the required image information to save";
    public static readonly string ImageNotFound = "Could not find image";
    public static readonly string ImageLoadFailed = "Failed to set image: invalid";
}