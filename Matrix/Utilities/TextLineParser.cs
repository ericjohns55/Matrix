using Matrix.Data;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Display;

namespace Matrix.Utilities;

public class TextLineParser
{
    public static string ParseTextLine(TextLine textLine)
    {
        string text = textLine.Text;

        foreach (var variable in ProgramState.CurrentVariables.Keys)
        {
            if (text.Contains(variable))
            {
                text = text.Replace(variable, ProgramState.CurrentVariables[variable]);
            }
        }

        return text;

        // MatrixFont font = textLine.Font;
        // int xPos = ParseXPosition(textLine, MatrixUpdater.MatrixWidth);
        // int yPos = ParseYPosition(textLine, MatrixUpdater.MatrixHeight);
        // need variable util for text
        // parse font to matrix font
        // parse color to matrix color
        // todo: write test class to test all of these
    }

    private static int ParseXPosition(TextLine textLine, int matrixWidth)
    {
        switch (textLine.XPositioning)
        {
            case Positioning.XPositioning.Center:
                return 3;
            case Positioning.XPositioning.CenterLeft:
                return 3;
            case Positioning.XPositioning.CenterRight:
                return 3;
            default:
                return textLine.XLocation;
        }
    }

    private static int ParseYPosition(TextLine textLine, int matrixHeight)
    {
        switch (textLine.XPositioning)
        {
            case Positioning.XPositioning.Center:
                return 3;
            case Positioning.XPositioning.CenterLeft:
                return 3;
            case Positioning.XPositioning.CenterRight:
                return 3;
            default:
                return textLine.YLocation;
        }
    }
}