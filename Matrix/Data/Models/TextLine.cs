namespace Matrix.Data.Models;

public class TextLine
{
    public int Id { get; init; }
    public string Text { get; init; }
    public MatrixColor Color { get; init; }
    public MatrixFont Font { get; init; }
}