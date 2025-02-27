using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Matrix.Data.Types;
using Matrix.Data.Utilities;
using Matrix.Display;
using Matrix.Utilities;
using RPiRgbLEDMatrix;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = RPiRgbLEDMatrix.Color;

namespace Matrix.Data.Models;

public class PlainText
{
    private class Line
    {
        public StringBuilder Builder { get; } = new StringBuilder();
        public int LineNumber { get; init; }
    }
    
    public string Text { get; init; }
    public string ParsedText { get; internal set; }
    public bool SplitByWord { get; init; }
    public TextAlignment TextAlignment { get; init; }
    public VerticalPositioning VerticalPositioning { get; init; }
    public MatrixColor Color { get; init; }
    public MatrixFont Font { get; init; }
    public SavedImage? Background { get; init; }
    private int MaxLineCount => MatrixUpdater.MatrixHeight / Font.Height;
    private int MaxCharactersPerLine => MatrixUpdater.MatrixWidth / Font.Width;

    [JsonIgnore]
    public Image<Rgb24>? BackgroundImage { get; init; }

    public bool ShouldUpdateSecondly => Text.Contains(VariableConstants.SecondVariable) ||
                                        Text.Contains(VariableConstants.TimerSecondVariable) ||
                                        Text.Contains(VariableConstants.TimerFormattedVariable);

    public PlainText(PlainTextPayload payload)
    {
        Text = payload.Text;
        TextAlignment = payload.TextAlignment;
        VerticalPositioning = payload.VerticalPositioning;
        SplitByWord = payload.SplitByWord;
        Font = payload.Font!;
        Color = payload.Color!;
        BackgroundImage = payload.BackgroundImage?.Image;
        Background = payload.BackgroundImage;
    }

    public List<ParsedTextLine> ParseIntoTextLines()
    {
        ParsedText = TextLineParser.SubstituteVariables(Text, ProgramState.CurrentVariables);
        
        var lines = ParseTextIntoLines();

        if (lines.Count == 0)
        {
            return new List<ParsedTextLine>();
        }

        var xPosition = GenerateXPositionCalculationFunction();
        var yPosition = GenerateYPositionCalculationFunction(lines.Count);

        RGBLedFont font = new RGBLedFont(Font.FileLocation);
        Color color = new Color(Color.Red, Color.Green, Color.Blue);
        
        var parsedLines = new List<ParsedTextLine>();
        foreach (var line in lines)
        {
            parsedLines.Add(new ParsedTextLine()
            {
                ParsedText = line.Builder.ToString().Trim(),
                XPosition = xPosition(line.Builder.Length),
                YPosition = yPosition(line.LineNumber),
                FontLocation = Font.FileLocation,
                FontHeight = Font.Height,
                FontWidth = Font.Width,
                Font = font,
                Color = color
            });
        }

        return parsedLines;
    }

    public Func<int, int> GenerateXPositionCalculationFunction()
    {
        switch (TextAlignment)
        {
            case TextAlignment.Left:
                return (_) => 0;
            case TextAlignment.Right:
                return (lineLength) => MatrixUpdater.MatrixWidth - lineLength * Font.Width;
            default:
                return (lineLength) => (MatrixUpdater.MatrixWidth - lineLength * Font.Width) / 2;
        }
    }

    public Func<int, int> GenerateYPositionCalculationFunction(int lineCount)
    {
        switch (VerticalPositioning)
        {
            case VerticalPositioning.Bottom:
                return (lineNumber) => MatrixUpdater.MatrixHeight - (lineCount * Font.Height) + (lineNumber * Font.Height);
            case VerticalPositioning.Center:
                var startPosition = (MatrixUpdater.MatrixHeight - (lineCount * Font.Height)) / 2;
                return (lineNumber) => startPosition + (lineNumber * Font.Height);
            case VerticalPositioning.Top:
                return (lineNumber) => (lineNumber * Font.Height);
        }
        
        throw new InvalidEnumArgumentException(nameof(VerticalPositioning), (int) VerticalPositioning, typeof(VerticalPositioning));
    }

    private List<Line> ParseTextIntoLines()
    {
        var lines = new List<Line>();
        int currentLine = 1;
        
        if (SplitByWord)
        {
            var allWords = ParsedText.Split(' ');

            foreach (var word in allWords)
            {
                if (lines.Count > MaxLineCount)
                {
                    break;
                }

                if (lines.Count < currentLine)
                {
                    lines.Add(new Line() { LineNumber = currentLine });
                }

                var line = lines[currentLine - 1];

                if (word.Length <= MaxCharactersPerLine)
                {
                    if (line.Builder.Length == 0)
                    {
                        line.Builder.Append(word);
                    }
                    else
                    {
                        if (line.Builder.Length + word.Length + 1 <= MaxCharactersPerLine)
                        {
                            line.Builder.Append($" {word}");
                        }
                        else
                        {
                            if (currentLine + 1 <= MaxLineCount)
                            {
                                currentLine++;
                                var nextLine = new Line() { LineNumber = currentLine };
                                nextLine.Builder.Append(word);

                                lines.Add(nextLine);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < ParsedText.Length; i += MaxCharactersPerLine)
            {
                if (lines.Count > MaxLineCount)
                {
                    break;
                }
                
                var newLine = new Line() { LineNumber = currentLine };

                var length = i + MaxCharactersPerLine < ParsedText.Length ? MaxCharactersPerLine : ParsedText.Length % MaxCharactersPerLine;
                
                newLine.Builder.Append(ParsedText.Substring(i, length));
                
                lines.Add(newLine);
                    
                currentLine++;
            }
        }

        return lines;
    }
}