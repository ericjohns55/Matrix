using Matrix.Data.Types;
using Matrix.Display;
using Matrix.Utilities;
using Newtonsoft.Json;
using RPiRgbLEDMatrix;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = RPiRgbLEDMatrix.Color;

namespace Matrix.Data.Models;

public class ScrollingText
{
    public string Text { get; }
    public MatrixColor Color { get; }
    public MatrixFont Font { get; }
    public SavedImage? Background { get; }
    public int ScrollingDelay { get; }
    public int Iterations { get; }
    
    public string ParsedText { get; set; }
    public VerticalPositioning VerticalPositioning { get; set; }
    private int YPosition { get; }
    private int MaxPosition { get; set; }
    
    public int CurrentPosition { get; set; }
    public int IterationsLeft { get; set; }

    [JsonIgnore]
    private RGBLedFont ParsedFont { get; init; }
    
    [JsonIgnore]
    private Color ParsedColor { get; init; }

    [JsonIgnore]
    private Image<Rgb24>? BackgroundImage { get; }

    public ScrollingText(ScrollingTextPayload payload)
    {
        Text = payload.Text;
        Color = payload.Color!;
        Font = payload.Font!;
        ScrollingDelay = payload.ScrollingDelay;
        Iterations = payload.Iterations;
        
        CurrentPosition = MatrixUpdater.MatrixWidth;
        IterationsLeft = Iterations;

        ParsedFont = new RGBLedFont(Font.FileLocation);
        ParsedColor = new Color(Color.Red, Color.Green, Color.Blue);

        Background = payload.BackgroundImage;
        BackgroundImage = payload.BackgroundImage?.Image;

        VerticalPositioning = payload.VerticalPositioning;
        switch (VerticalPositioning)
        {
            case VerticalPositioning.Bottom:
                YPosition = MatrixUpdater.MatrixHeight;
                break;
            case VerticalPositioning.Center:
                YPosition = MatrixUpdater.MatrixHeight / 2 + Font.Height / 2;
                break;
            case VerticalPositioning.Top:
                YPosition = Font.Height;
                break;
        }
        
        ParseText();
    }
    
    public Image<Rgb24>? GetBackgroundImage() => BackgroundImage;

    private void ParseText()
    {
        ParsedText = TextLineParser.SubstituteVariables(Text, ProgramState.CurrentVariables).Trim();
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
            FontHeight = Font.Height,
            FontWidth = Font.Width,
        };
    }
}