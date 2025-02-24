using Matrix.Data.Models;

namespace Matrix.Data.Utilities;

public static class MatrixEntityExtensions
{
    public static PlainTextPayload DeepCopy(this PlainTextPayload payload)
    {
        return new PlainTextPayload()
        {
            Id = payload.Id,
            Text = payload.Text,
            SplitByWord = payload.SplitByWord,
            MatrixColorId = payload.MatrixColorId,
            MatrixFontId = payload.MatrixFontId,
            Color = payload.Color?.DeepCopy(),
            Font = payload.Font?.DeepCopy()
        };
    }
    
    public static ScrollingTextPayload DeepCopy(this ScrollingTextPayload payload)
    {
        return new ScrollingTextPayload()
        {
            Id = payload.Id,
            Iterations = payload.Iterations,
            ScrollingDelay = payload.ScrollingDelay,
            MatrixColorId = payload.MatrixColorId,
            MatrixFontId = payload.MatrixFontId,
            Color = payload.Color?.DeepCopy(),
            Font = payload.Font?.DeepCopy()
        };
    }
    
    public static ClockFace DeepCopy(this ClockFace face)
    {
        return new ClockFace()
        {
            Id = face.Id,
            Name = face.Name,
            TextLines = face.TextLines.Select(line => line.DeepCopy()).ToList(),
            TimePeriods = face.TimePeriods.Select(timePeriod => timePeriod.DeepCopy()).ToList(),
            IsTimerFace = face.IsTimerFace,
            Deleted = face.Deleted
        };
    }
    
    private static MatrixColor DeepCopy(this MatrixColor color)
    {
        return new MatrixColor()
        {
            Id = color.Id,
            Name = color.Name,
            Red = color.Red,
            Green = color.Green,
            Blue = color.Blue
        };
    }

    private static MatrixFont DeepCopy(this MatrixFont font)
    {
        return new MatrixFont()
        {
            Id = font.Id,
            Name = font.Name,
            FileLocation = font.FileLocation,
        };
    }
    
    private static TextLine DeepCopy(this TextLine textLine)
    {
        return new TextLine()
        {
            Id = textLine.Id,
            Text = textLine.Text,
            XLocation = textLine.XLocation,
            YLocation = textLine.YLocation,
            XPositioning = textLine.XPositioning,
            YPositioning = textLine.YPositioning,
            MatrixColorId = textLine.MatrixColorId,
            MatrixFontId = textLine.MatrixFontId,
            ClockFaceId = textLine.ClockFaceId,
        };
    }

    private static TimePeriod DeepCopy(this TimePeriod timePeriod)
    {
        return new TimePeriod()
        {
            Id = timePeriod.Id,
            ClockFaceId = timePeriod.ClockFaceId,
            StartHour = timePeriod.StartHour,
            StartMinute = timePeriod.StartMinute,
            StartSecond = timePeriod.StartSecond,
            EndHour = timePeriod.EndHour,
            EndMinute = timePeriod.EndMinute,
            EndSecond = timePeriod.EndSecond,
            DaysOfWeek = timePeriod.DaysOfWeek.Select(day => day).ToList(),
        };
    }
}