using Matrix.Data.Types;

namespace Matrix.Data.Models.Web;

public class PlainTextPayload
{
    public string Text { get; init; } = string.Empty;
    public TextAlignment TextAlignment { get; init; } = TextAlignment.Center;
    public bool SplitByWord { get; init; } = true;

    public int MatrixColorId { get; init; }
    public MatrixColor? Color { get; set; }
    
    public int MatrixFontId { get; init; }
    public MatrixFont? Font { get; set; }
}