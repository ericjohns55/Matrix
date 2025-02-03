using Matrix.Data.Models.Web;
using Matrix.Display;
using Matrix.Utilities;
using RPiRgbLEDMatrix;

namespace Matrix.Data.Models;

public class ScrollingText
{
    private string Text { get; }
    private MatrixColor Color { get; }
    private MatrixFont Font { get; }
    public int ScrollingDelay { get; }
    private int Iterations { get; }
    
    private string ParsedText { get; set; }
    private int YPosition { get; }
    private int MaxPosition { get; set; }
    
    private int CurrentPosition { get; set; }
    private int IterationsLeft { get; set; }

    private RGBLedFont ParsedFont { get; init; }
    private Color ParsedColor { get; init; }

    public ScrollingText(ScrollingTextPayload payload)
    {
        Text = payload.Text;
        Color = payload.Color!;
        Font = payload.Font!;
        ScrollingDelay = payload.ScrollingDelay;
        Iterations = payload.Iterations;
        
        CurrentPosition = MatrixUpdater.MatrixWidth;
        IterationsLeft = Iterations;
        YPosition = MatrixUpdater.MatrixHeight / 2 + Font.Height / 2;

        ParsedFont = new RGBLedFont(Font.FileLocation);
        ParsedColor = new Color(Color.Red, Color.Green, Color.Blue);
        
        ParseText();
    }

    private void ParseText()
    {
        ParsedText = TextLineParser.SubstituteVariables(Text, ProgramState.CurrentVariables);
        MaxPosition = -1 * Font.Width * ParsedText.Length - MatrixUpdater.MatrixWidth;
    }

    /// <summary>
    ///     Ticks the scrolling text, returns true if there will be more updates, false if we are complete
    /// </summary>
    public bool HandleUpdate()
    {
        ParseText();
        
        CurrentPosition--;

        if (CurrentPosition <= MaxPosition)
        {
            if (Iterations != -1)
            {
                IterationsLeft--;
            }
            
            CurrentPosition = MatrixUpdater.MatrixWidth;
        }

        if (Iterations != -1 && IterationsLeft <= 0)
        {
            return false;
        }

        return true;
    }

    public ParsedTextLine GetParsedTextLine()
    {
        return new ParsedTextLine()
        {
            XPosition = CurrentPosition,
            YPosition = YPosition,
            ParsedText = ParsedText,
            Color = ParsedColor,
            Font = ParsedFont,
            FontLocation = Font.FileLocation,
            FontHeight = Font.Height
        };
    }
}