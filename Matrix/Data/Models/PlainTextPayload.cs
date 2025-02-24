using Matrix.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class PlainTextPayload
{
    public int Id { get; init; }
    public string Text { get; set; } = string.Empty;
    public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;
    public bool SplitByWord { get; set; } = true;

    public int MatrixColorId { get; set; }
    public MatrixColor? Color { get; set; }
    
    public int MatrixFontId { get; set; }
    public MatrixFont? Font { get; set; }
}