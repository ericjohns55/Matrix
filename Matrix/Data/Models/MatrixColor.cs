using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
[Index(nameof(Name), IsUnique = true)]
public class MatrixColor
{
    public int Id { get; init; }
    
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public int Red { get; set; }
    
    public int Green { get; set; }
    
    public int Blue { get; set; }
    
    public bool Deleted { get; set; }
}