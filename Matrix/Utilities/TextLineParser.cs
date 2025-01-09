using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Display;

namespace Matrix.Utilities;

public class TextLineParser
{
    public static void ParseTextLine(TextLine textLine)
    {
        int xPos = ParseXPosition(textLine);
        int yPos = ParseYPosition(textLine);
        // need variable util for text
        // parse font to matrix font
        // parse color to matrix color
        // todo: write test class to test all of these
    }

    public static int ParseXPosition(TextLine textLine)
    {
        var matrixWidth = MatrixUpdater.MatrixWidth;

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

    public static int ParseYPosition(TextLine textLine)
    {
        var matrixHeight = MatrixUpdater.MatrixWidth;

        return matrixHeight;
    }
}