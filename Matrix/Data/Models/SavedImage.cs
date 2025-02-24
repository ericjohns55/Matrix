using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.Data.Models;

[PrimaryKey(nameof(Id))]
public class SavedImage
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string FileName { get; set; }

    [NotMapped]
    public string? Base64Rendering { get; set; } = null;

    [NotMapped]
    public string? ScaledRendering { get; set; } = null;

    [NotMapped]
    [JsonIgnore]
    public Image<Rgb24>? Image { get; set; } = null;
}