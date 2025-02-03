namespace Matrix.Data.Models;

public class ParsedTextLine
{
    public string ParsedText { get; init; }
    public int XPosition { get; init; }
    public int YPosition { get; init; }
    public int? FontHeight { get; init; }
    public string? FontLocation { get; init; }
    public RPiRgbLEDMatrix.RGBLedFont Font { get; init; }
    public RPiRgbLEDMatrix.Color Color { get; init; }
}