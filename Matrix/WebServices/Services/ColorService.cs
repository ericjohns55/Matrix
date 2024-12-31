using Matrix.Data.Models;
using Matrix.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class ColorService : IColorService
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

    public Task<MatrixColor?> GetMatrixColor(string colorName)
    {
        return _matrixContext.MatrixColor.FirstOrDefaultAsync(color => color.Name == colorName);
    }

    public async Task<MatrixColor?> UpdateMatrixColor(int colorId, MatrixColor color)
    {
        _matrixContext.MatrixColor.Update(color);
        await _matrixContext.SaveChangesAsync();
        return await GetMatrixColor(colorId);
    }

    public async Task<MatrixColor?> AddMatrixColor(MatrixColor color)
    {
        await _matrixContext.MatrixColor.AddAsync(color);
        await _matrixContext.SaveChangesAsync();
        
        return await _matrixContext.MatrixColor.FirstOrDefaultAsync(c => c.Name == color.Name);
    }

    public async Task<int> RemoveMatrixColor(int colorId)
    {
        MatrixColor? color = await GetMatrixColor(colorId);
        
        if (color != null)
        {
            _matrixContext.MatrixColor.Remove(color);
        }
        
        await _matrixContext.SaveChangesAsync();
        return colorId;
    }
}