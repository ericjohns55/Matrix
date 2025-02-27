using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Utilities;
using Matrix.Display;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.WebServices.Services;

public class ImageService
{
    private ILogger<ImageService> _logger;
    private MatrixContext _matrixContext;
    private string _imagesFolderPath;

    public ImageService(MatrixContext matrixContext, ILogger<ImageService> logger)
    {
        _logger = logger;
        _matrixContext = matrixContext;
        _imagesFolderPath = Path.Combine(Environment.CurrentDirectory, "Data", ConfigConstants.SavedImagesFolder);
    }

    public async Task<List<SavedImage>> GetSavedImages(int scaleFactor = 0, bool trimHeader = false, string? filePath = null)
    {
        var images = await _matrixContext.SavedImage.OrderBy(image => image.Name).ToListAsync();

        foreach (var image in images)
        {
            var rawImage = Image.Load<Rgb24>(Path.Combine(filePath ?? _imagesFolderPath, image.FileName));
            image.Base64Rendering = MatrixRenderer.ImageToBase64(rawImage, trimHeader);

            if (scaleFactor > 1)
            {
                var scaledImage = MatrixRenderer.OptionallyScaleImage(rawImage, scaleFactor);
                image.ScaledRendering = MatrixRenderer.ImageToBase64(scaledImage, trimHeader);
            }
        }
        
        return images;
    }

    public async Task<SavedImage> SaveImage(ImagePayload imagePayload, string? filePath = null)
    {
        if (imagePayload == null)
        {
            throw new ArgumentNullException(WebConstants.ImageNotFound);
        }
        
        if (string.IsNullOrWhiteSpace(imagePayload.Base64Image) || string.IsNullOrWhiteSpace(imagePayload.ImageName))
        {
            throw new MatrixEntityNotValidException(WebConstants.MissingImageInformation);
        }
        
        string fileName = await SaveToFile(imagePayload, filePath);

        var savedImage = new SavedImage()
        {
            Name = imagePayload.ImageName!,
            FileName = fileName,
        };
        
        await _matrixContext.SavedImage.AddAsync(savedImage);
        await _matrixContext.SaveChangesAsync();

        return savedImage;
    }

    public async Task<string> GetBase64OfImageById(int imageId, bool trimHeader = false)
    {
        return MatrixRenderer.ImageToBase64((await GetImageById(imageId, true)).Image, trimHeader);
    }

    public async Task<SavedImage> GetImageById(int imageId, bool includeRendering = false, string? filePath = null)
    {
        var image = await _matrixContext.SavedImage.FirstOrDefaultAsync(image => image.Id == imageId);

        if (image == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.ImageNotFound);
        }

        if (includeRendering)
        {
            image.Image = await Image.LoadAsync<Rgb24>(Path.Combine(filePath ?? _imagesFolderPath, image.FileName));
        }

        return image;
    }

    public async Task<SavedImage> UpdateImage(int imageId, ImagePayload imagePayload, string? filePath = null)
    {
        var image = await GetImageById(imageId);
        
        if (imagePayload == null)
        {
            throw new ArgumentNullException($"{imagePayload} cannot be null");
        }

        if (imagePayload.Base64Image == null && imagePayload.ImageName == null)
        {
            throw new MatrixEntityNotValidException(WebConstants.MissingImageInformation);
        }

        if (!string.IsNullOrWhiteSpace(imagePayload.ImageName))
        {
            image.Name = imagePayload.ImageName!;
        }

        if (!string.IsNullOrWhiteSpace(imagePayload.Base64Image))
        {
            await QuietlyDeleteFile(Path.Combine(filePath ?? _imagesFolderPath, image.FileName));
            image.FileName = await SaveToFile(imagePayload, filePath ?? _imagesFolderPath);
        }
        
        _matrixContext.Update(image);
        await _matrixContext.SaveChangesAsync();

        return image;
    }

    public async Task<SavedImage> DeleteImage(int id, string? filePath = null)
    {
        var image = await GetImageById(id);

        await QuietlyDeleteFile(Path.Combine(filePath ?? _imagesFolderPath, image.FileName));
        
        _matrixContext.SavedImage.Remove(image);
        await _matrixContext.SaveChangesAsync();

        return image;
    }

    public Image<Rgb24> GetImageFromBase64(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        return Image.Load<Rgb24>(imageBytes);
    }

    private async Task<string> SaveToFile(ImagePayload imagePayload, string? filePath = null)
    {
        var image = GetImageFromBase64(imagePayload.Base64Image!);
        
        var fileName = $"{Guid.NewGuid()}.png";
        await image.SaveAsPngAsync(Path.Combine(filePath ?? _imagesFolderPath, fileName));

        return fileName;
    }

    private Task QuietlyDeleteFile(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }
}