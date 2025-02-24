using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class ScrollingTextPayload
{
    public int Id { get; init; }
    public string Text { get; set; } = string.Empty;
    public int MatrixColorId { get; set; }
    public MatrixColor? Color { get; set; }
    public int MatrixFontId { get; set; }
    public MatrixFont? Font { get; set; }
    public int ScrollingDelay { get; set; }
    public int Iterations { get; set; }
}