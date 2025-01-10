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

    public int Width => int.Parse(Name.Split('x')[0].Where(c => char.IsDigit(c)).ToArray());
    
    public int Height => int.Parse(Name.Split('x')[1].Where(c => char.IsDigit(c)).ToArray());
}