using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
[Index(nameof(Name), IsUnique = true)]
public class MatrixFont
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public string Name { get; set; }
    
    [MaxLength(512)]
    public string FileLocation { get; set; }
}