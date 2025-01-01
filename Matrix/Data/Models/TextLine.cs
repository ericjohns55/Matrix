using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class TextLine
{
    public int Id { get; init; }
    
    public string Text { get; set; }

    public int XLocation { get; set; }
    public int YLocation { get; set; }
    public bool RelativeLocation { get; set; }

    public int MatrixColorId { get; set; }
    public MatrixColor Color { get; set; }
    
    public int MatrixFontId { get; set; }
    public MatrixFont Font { get; set; }
    
    public int ClockFaceId { get; set; }
}