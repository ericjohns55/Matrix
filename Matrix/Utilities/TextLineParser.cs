using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Display;
using RPiRgbLEDMatrix;

namespace Matrix.Utilities;

public class TextLineParser
{
    public static ParsedTextLine ParseTextLine(TextLine textLine, Dictionary<string, string> currentVariables)
    {
        string parsedText = SubstituteVariables(textLine.Text, currentVariables);
        
        parsedText = parsedText.Substring(0,
            Math.Min(parsedText.Length, GetMaxSubstringLength(textLine, MatrixUpdater.MatrixWidth)));
        
        int xPos = ParseXPosition(textLine, parsedText, MatrixUpdater.MatrixWidth);
        int yPos = ParseYPosition(textLine, MatrixUpdater.MatrixHeight);

        RGBLedFont font = new RGBLedFont(textLine.Font.FileLocation);
        Color color = new Color(textLine.Color.Red, textLine.Color.Green, textLine.Color.Blue);

        return new ParsedTextLine()
        {
            ParsedText = parsedText,
            XPosition = xPos,
            YPosition = yPos,
            Font = font,
            Color = color
        };
    }
    
    public static string SubstituteVariables(string text, Dictionary<string, string> variables)
    {
        string parsedText = text;

        foreach (var variable in variables.Keys)
        {
            if (parsedText.Contains(variable))
            {
                parsedText = parsedText.Replace(variable, variables[variable]);
            }
        }

        return parsedText;
    }

    private static int ParseXPosition(TextLine textLine, string parsedText, int matrixWidth)
    {
        var textLength = parsedText.Length;
        var textLengthInPixels = textLength * textLine.Font.Width;
        
        switch (textLine.XPositioning)
        {
            case Positioning.XPositioning.Center:
                return (matrixWidth - textLengthInPixels) / 2;
            case Positioning.XPositioning.CenterLeft:
                return (matrixWidth / 2 - textLengthInPixels) / 2;
            case Positioning.XPositioning.CenterRight:
                return ((matrixWidth / 2 - textLengthInPixels) / 2) + matrixWidth / 2;
            default:
                return textLine.XLocation;
        }
    }

    private static int GetMaxSubstringLength(TextLine textLine, int matrixWidth)
    {
        var maxFontLength = (int) Math.Floor(matrixWidth / (double) textLine.Font.Width);
        int substringLength = 0;
        
        switch (textLine.XPositioning)
        {
            case Positioning.XPositioning.Center:
                substringLength = maxFontLength;
                break;
            case Positioning.XPositioning.CenterLeft:
            case Positioning.XPositioning.CenterRight:
                substringLength = maxFontLength / 2;
                break;
            case Positioning.XPositioning.Absolute:
                substringLength = (int) Math.Floor((matrixWidth - textLine.XLocation) / (double)textLine.Font.Width);
                break;
        }
        
        return substringLength;
    }

    private static int ParseYPosition(TextLine textLine, int matrixHeight)
    {
        var halfOfFontHeight = textLine.Font.Height / 2;
        
        switch (textLine.YPositioning)
        {
            case Positioning.YPositioning.Center:
                return matrixHeight / 2 + halfOfFontHeight;
            case Positioning.YPositioning.TopQuarter:
                return matrixHeight / 4 + halfOfFontHeight;
            case Positioning.YPositioning.BottomQuarter:
                return 3 * matrixHeight / 4 + halfOfFontHeight;
            default:
                return textLine.YLocation;
        }
    }
}