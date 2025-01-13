using System.ComponentModel.DataAnnotations;
using Matrix.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class TextLine
{
    public int Id { get; init; }
    
    [Required] public string Text { get; set; }

    public int XLocation { get; init; }
    public int YLocation { get; init; }
    public Positioning.XPositioning XPositioning { get; init; }
    public Positioning.YPositioning YPositioning { get; init; }

    public int MatrixColorId { get; init; }
    public MatrixColor Color { get; init; }
    
    public int MatrixFontId { get; init; }
    public MatrixFont Font { get; init; }
    
    public int ClockFaceId { get; init; }
}