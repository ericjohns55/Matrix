namespace Matrix.Data.Models.Web;

public class ScrollingTextPayload
{
    public string Text { get; init; } = string.Empty;
    public int MatrixColorId { get; init; }
    public MatrixColor? Color { get; set; }
    public int MatrixFontId { get; init; }
    public MatrixFont? Font { get; set; }
    public int ScrollingDelay { get; init; }
    public int Iterations { get; init; }
}