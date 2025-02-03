using BdfFontParser;
using Matrix.Data;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Utilities;
using Matrix.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.Display;

public static class MatrixRenderer
{
    public static Image<Rgb24> RenderClockFace(ClockFace? clockFace) 
    {
        if (clockFace == null)
        {
            throw new ClockFaceException(WebConstants.ClockFaceNull);
        }

        var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);

        foreach (var textLine in clockFace.TextLines)
        {
            var parsedLine = TextLineParser.ParseTextLine(textLine, ProgramState.CurrentVariables);
            DrawParsedTextLine(image, parsedLine);
        }

        return image;
    }

    public static Image<Rgb24> RenderPlainText(PlainText? plainText)
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

        return image;
    }

    public static Image<Rgb24> RenderScrollingText(ScrollingText? scrollingText)
    {
        if (scrollingText == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.TextNotFound);
        }
        
        var image = new Image<Rgb24>(MatrixUpdater.MatrixWidth, MatrixUpdater.MatrixHeight);

        DrawParsedTextLine(image, scrollingText.GetParsedTextLine(), true);

        return image;
    }

    private static void DrawParsedTextLine(Image<Rgb24> image, ParsedTextLine parsedTextLine, bool resetX = false)
    {
        if (parsedTextLine.FontHeight == null || parsedTextLine.FontHeight == null)
        {
            throw new FontException(WebConstants.MissingFontInformation);
        }
        
        BdfFont font = new BdfFont(parsedTextLine.FontLocation!);
        var mapping = font.GetMapOfString(parsedTextLine.ParsedText);
        
        var imageXOffset = resetX ? 0 : parsedTextLine.XPosition;
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