using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class ColorService
{
    private readonly MatrixContext _matrixContext;

    public ColorService(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }

    public Task<List<MatrixColor>> GetMatrixColors(SearchFilter filter = SearchFilter.Active)
    {
        if (filter == SearchFilter.AllResults)
        {
            return _matrixContext.MatrixColor.ToListAsync();
        }

        var searchForDeleted = filter == SearchFilter.Deleted;
        
        return _matrixContext.MatrixColor.Where(color => color.Deleted == searchForDeleted).ToListAsync();
    }

    public Task<MatrixColor?> GetMatrixColor(int colorId)
    {
        return _matrixContext.MatrixColor.FirstOrDefaultAsync(color => color.Id == colorId);
    }

    public async Task<MatrixColor?> UpdateMatrixColor(int colorId, MatrixColor updatedColor)
    {
        var originalColor = await GetMatrixColor(colorId);

        if (originalColor == null)
        {
            throw new MatrixEntityNotFoundException($"Color with id {colorId} not found");
        }

        if (updatedColor == null)
        {
            throw new ArgumentNullException($"{updatedColor} cannot be null");
        }
        
        originalColor.Name = updatedColor.Name;
        originalColor.Red = updatedColor.Red;
        originalColor.Green = updatedColor.Green;
        originalColor.Blue = updatedColor.Blue;
        
        _matrixContext.MatrixColor.Update(originalColor);
        await _matrixContext.SaveChangesAsync();
        
        return await GetMatrixColor(colorId);
    }

    public async Task<MatrixColor?> AddMatrixColor(MatrixColor color)
    {
        if (color == null)
        {
            throw new ArgumentNullException($"{color} cannot be null");
        }
        
        await _matrixContext.MatrixColor.AddAsync(color);
        await _matrixContext.SaveChangesAsync();
        
        return await _matrixContext.MatrixColor.FirstOrDefaultAsync(c => c.Name == color.Name);
    }

    public async Task<int> RemoveMatrixColor(int colorId)
    {
        MatrixColor? color = await GetMatrixColor(colorId);

        if (color == null)
        {
            throw new MatrixEntityNotFoundException($"Color with id {colorId} not found");
        }
        
        color.Deleted = true;
        await _matrixContext.SaveChangesAsync();
        
        return color.Id;
    }

    public async Task<MatrixColor> RestoreMatrixColor(int colorId)
    {
        MatrixColor? color = await GetMatrixColor(colorId);

        if (color == null)
        {
            throw new MatrixEntityNotFoundException($"Color with id {colorId} was never deleted");
        }
        
        color.Deleted = false;
        await _matrixContext.SaveChangesAsync();
        
        return color;
    }
}