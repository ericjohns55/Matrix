using BdfFontParser;
using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Utilities;
using Matrix.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.Display;

public static class MatrixRenderer
{
    public static Image<Rgb24> RenderClockFace(ClockFace? clockFace, int scaleFactor = 1, bool useCurrentVariables = true) 
    {
        if (clockFace == null)
        {
            throw new ClockFaceException(WebConstants.ClockFaceNull);
        }

        var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);

        foreach (var textLine in clockFace.TextLines)
        {
            var variables = useCurrentVariables ? ProgramState.CurrentVariables : VariableUtility.GetDefaultVariables();
            var parsedLine = TextLineParser.ParseTextLine(textLine, variables);
            DrawParsedTextLine(image, parsedLine);
        }

        return OptionallyScaleImage(image, scaleFactor);
    }

    public static Image<Rgb24> RenderTimer(Timer? timer, ClockFace? clockFace, int scaleFactor = 1)
    {
        if (clockFace == null)
        {
            throw new ClockFaceException(WebConstants.ClockFaceNull);
        }

        if (timer == null)
        {
            throw new TimerException(WebConstants.TimerNull);
        }
        
        var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);
        
        // copying variables
        var variables = ProgramState.CurrentVariables.ToDictionary(v => v.Key, v => v.Value);
        variables[VariableConstants.TimerHourVariable] = timer.Hour.ToString();
        variables[VariableConstants.TimerMinuteVariable] = timer.Minute.ToString("D2");
        variables[VariableConstants.TimerSecondVariable] = timer.Second.ToString("D2");
        variables[VariableConstants.TimerFormattedVariable] = new MatrixTimer(timer).GetFormattedTimer();

        foreach (var textLine in clockFace.TextLines)
        {
            var parsedLine = TextLineParser.ParseTextLine(textLine, variables);
            DrawParsedTextLine(image, parsedLine);
        }

        return OptionallyScaleImage(image, scaleFactor);
    }

    public static Image<Rgb24> RenderPlainText(PlainText? plainText, int scaleFactor = 1)
    {
        if (plainText == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.TextNotFound);
        }
        
        var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);

        foreach (var parsedTextLine in plainText.ParseIntoTextLines())
        {
            DrawParsedTextLine(image, parsedTextLine);
        }

        return OptionallyScaleImage(image, scaleFactor);
    }

    public static Image<Rgb24> RenderScrollingText(ScrollingText? scrollingText, int scaleFactor = 1, bool cropToMatrixSize = true)
    {
        if (scrollingText == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.TextNotFound);
        }

        var parsedTextLine = scrollingText.GetParsedTextLine();
        
        int width = MatrixUpdater.MatrixWidth;

        if (!cropToMatrixSize && parsedTextLine.FontWidth != null)
        {
            width += parsedTextLine.ParsedText.Length * parsedTextLine.FontWidth.Value;
        }
        
        var image = new Image<Rgb24>(width, MatrixUpdater.MatrixHeight);

        DrawParsedTextLine(image, parsedTextLine, true);

        return OptionallyScaleImage(image, scaleFactor);
    }

    public static Image<Rgb24> RenderImage(Image<Rgb24>? image, int scaleFactor = 1)
    {
        if (image == null)
        {
            return new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);
        }
        
        return OptionallyScaleImage(image, scaleFactor);
    }

    private static void DrawParsedTextLine(Image<Rgb24> image, ParsedTextLine parsedTextLine, bool resetX = false)
    {
        if (parsedTextLine.FontHeight == null)
        {
            throw new FontException(WebConstants.MissingFontInformation);
        }
        
        BdfFont font = new BdfFont(parsedTextLine.FontLocation!);
        var mapping = font.GetMapOfString(parsedTextLine.ParsedText);
        
        var imageXOffset = resetX ? 0 : parsedTextLine.XPosition + 1;
        var imageYOffset = parsedTextLine.YPosition - parsedTextLine.FontHeight!.Value + 1;
        
        var textColor = new Rgb24(
            Convert.ToByte(parsedTextLine.Color.R), 
            Convert.ToByte(parsedTextLine.Color.G),
            Convert.ToByte(parsedTextLine.Color.B));
        
        for (int i = 0; i < mapping.GetLength(1); i++)
        {
            for (int j = 0; j < mapping.GetLength(0); j++)
            {
                if (mapping[j, i])
                {
                    if (j + imageXOffset < image.Width && i + imageYOffset < image.Height)
                    {
                        image[j + imageXOffset, i + imageYOffset] = textColor;
                    }
                }
            }
        }
    }

    private static Image<Rgb24> OptionallyScaleImage(Image<Rgb24> image, int scaleFactor)
    {
        if (scaleFactor <= 1)
        {
            return image;
        }
        
        var scaledImage = new Image<Rgb24>(image.Width * scaleFactor, image.Height * scaleFactor);

        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                if (image[i, j].R != 0 || image[i, j].G != 0 || image[i, j].B != 0)
                {
                    for (int x = 0; x < scaleFactor; x++)
                    {
                        for (int y = 0; y < scaleFactor; y++)
                        {
                            scaledImage[i * scaleFactor + x, j * scaleFactor + y] = image[i, j];
                        }
                    }
                }
            }
        }
        
        return scaledImage;
    }

    public static string ImageToBase64(Image<Rgb24>? image, bool trimHeader = false)
    {
        if (image == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.MissingImage);
        }
        
        string base64Encoding = image.ToBase64String(PngFormat.Instance);

        if (trimHeader)
        {
            base64Encoding = base64Encoding.Replace("data:image/png;base64,", string.Empty);
        }

        return base64Encoding;
    }
}