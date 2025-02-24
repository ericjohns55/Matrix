using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class TextService
{
    private readonly MatrixContext _matrixContext;

    public TextService(MatrixContext matrixContext)
    {
        _matrixContext = matrixContext;
    }

    public async Task LoadPlainTextDependencies(PlainTextPayload plainTextPayload)
    {
        if (plainTextPayload.Color == null)
        {
            plainTextPayload.Color = await LoadMatrixColorFromId(plainTextPayload.MatrixColorId);
        }

        if (plainTextPayload.Font == null)
        {
            plainTextPayload.Font = await LoadMatrixFontFromId(plainTextPayload.MatrixFontId);
        }
        
        if (plainTextPayload.Color == null || plainTextPayload.Font == null)
        {
            throw new MatrixEntityNotValidException(WebConstants.PlainTextNotValid);
        }
    }

    public async Task<List<PlainTextPayload>> GetSavedPlainText()
    {
        return await _matrixContext.SavedPlainText
            .Include(text => text.Color)
            .Include(text => text.Font)
            .OrderBy(text => text.Text)
            .ToListAsync();
    }
    
    public async Task<PlainTextPayload> GetPlainTextById(int plainTextId)
    {
        var plainTextPayload = await _matrixContext.SavedPlainText.Where(text => text.Id == plainTextId)
            .Include(text => text.Color)
            .Include(text => text.Font)
            .SingleOrDefaultAsync();

        if (plainTextPayload == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.PlainTextNotFound);
        }
            
        if (plainTextPayload.Color == null || plainTextPayload.Font == null)
        {
            throw new MatrixEntityNotValidException(WebConstants.PlainTextNotValid);
        }
        
        return plainTextPayload;
    }

    public async Task<PlainTextPayload> SavePlainText(PlainTextPayload payload)
    {
        if (payload == null)
        {
            throw new ArgumentNullException($"{payload} cannot be null");
        }
        
        await LoadPlainTextDependencies(payload);
        
        await _matrixContext.SavedPlainText.AddAsync(payload);
        await _matrixContext.SaveChangesAsync();

        return await GetPlainTextById(payload.Id);
    }

    public async Task<PlainTextPayload> DeletePlainText(int plainTextId)
    {
        var plainText = await GetPlainTextById(plainTextId);
        
        _matrixContext.SavedPlainText.Remove(plainText);
        await _matrixContext.SaveChangesAsync();
        
        return plainText;
    }

    public async Task<PlainTextPayload> UpdatePlainText(int plainTextId, PlainTextPayload payload)
    {
        if (payload == null)
        {
            throw new ArgumentNullException($"{payload} cannot be null");
        }
        
        var plainText = await GetPlainTextById(plainTextId);
        
        plainText.Text = payload.Text;
        plainText.TextAlignment = payload.TextAlignment;
        plainText.SplitByWord = payload.SplitByWord;

        if (payload.MatrixColorId == 0 && payload.Color != null)
        {
            _matrixContext.Add(payload.Color);
            plainText.Color = payload.Color;
        }
        else
        {
            plainText.MatrixColorId = payload.MatrixColorId;
        }

        if (payload.MatrixFontId == 0 && payload.Font != null)
        {
            _matrixContext.Add(payload.Font);
            plainText.Font = payload.Font;
        }
        else
        {
            plainText.MatrixFontId = payload.MatrixFontId;
        }

        _matrixContext.SavedPlainText.Update(plainText);
        await _matrixContext.SaveChangesAsync();
        
        return await GetPlainTextById(plainTextId);
    }
    
    public async Task LoadScrollingTextDependencies(ScrollingTextPayload scrollingTextPayload)
    {
        if (scrollingTextPayload.Color == null)
        {
            scrollingTextPayload.Color = await LoadMatrixColorFromId(scrollingTextPayload.MatrixColorId);
        }

        if (scrollingTextPayload.Font == null)
        {
            scrollingTextPayload.Font = await LoadMatrixFontFromId(scrollingTextPayload.MatrixFontId);
        }
        
        if (scrollingTextPayload.Color == null || scrollingTextPayload.Font == null)
        {
            throw new MatrixEntityNotValidException(WebConstants.ScrollingTextNotValid);
        }
    }
    
    public async Task<List<ScrollingTextPayload>> GetSavedScrollingText()
    {
        return await _matrixContext.SavedScrollingText
            .Include(text => text.Color)
            .Include(text => text.Font)
            .OrderBy(text => text.Text)
            .ToListAsync();
    }
    
    public async Task<ScrollingTextPayload> GetScrollingTextById(int scrollingTextId)
    {
        var scrollingTextPayload = await _matrixContext.SavedScrollingText.Where(text => text.Id == scrollingTextId)
            .Include(text => text.Color)
            .Include(text => text.Font)
            .SingleOrDefaultAsync();

        if (scrollingTextPayload == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.ScrollingTextNotFound);
        }
            
        if (scrollingTextPayload.Color == null || scrollingTextPayload.Font == null)
        {
            throw new MatrixEntityNotValidException(WebConstants.ScrollingTextNotValid);
        }
        
        return scrollingTextPayload;
    }

    public async Task<List<ScrollingTextPayload>> SaveScrollingText(ScrollingTextPayload payload)
    {
        if (payload == null)
        {
            throw new ArgumentNullException($"{payload} cannot be null");
        }
        
        await LoadScrollingTextDependencies(payload);
        
        await _matrixContext.SavedScrollingText.AddAsync(payload);
        await _matrixContext.SaveChangesAsync();

        return await GetSavedScrollingText();
    }

    public async Task<ScrollingTextPayload> DeleteScrollingText(int scrollingTextId)
    {
        var scrollingText = await GetScrollingTextById(scrollingTextId);
        
        _matrixContext.SavedScrollingText.Remove(scrollingText);
        await _matrixContext.SaveChangesAsync();
        
        return scrollingText;
    }
    public async Task<ScrollingTextPayload> UpdateScrollingText(int scrollingTextId, ScrollingTextPayload payload)
    {
        if (payload == null)
        {
            throw new ArgumentNullException($"{payload} cannot be null");
        }
        
        var scrollingText = await GetScrollingTextById(scrollingTextId);
        
        scrollingText.Text = payload.Text;
        scrollingText.ScrollingDelay = payload.ScrollingDelay;
        scrollingText.Iterations = payload.Iterations;

        if (payload.MatrixColorId == 0 && payload.Color != null)
        {
            _matrixContext.Add(payload.Color);
            scrollingText.Color = payload.Color;
        }
        else
        {
            scrollingText.MatrixColorId = payload.MatrixColorId;
        }

        if (payload.MatrixFontId == 0 && payload.Font != null)
        {
            _matrixContext.Add(payload.Font);
            scrollingText.Font = payload.Font;
        }
        else
        {
            scrollingText.MatrixFontId = payload.MatrixFontId;
        }
        
        _matrixContext.SavedScrollingText.Update(scrollingText);
        await _matrixContext.SaveChangesAsync();
        
        return await GetScrollingTextById(scrollingTextId);
    }
    
    private async Task<MatrixColor> LoadMatrixColorFromId(int id)
    {
        var color = await _matrixContext.MatrixColor.FirstOrDefaultAsync(color => color.Id == id);

        if (color == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.ColorNotFound);
        }

        return color;
    }

    private async Task<MatrixFont> LoadMatrixFontFromId(int id)
    {
        var font = await _matrixContext.MatrixFont.FirstOrDefaultAsync(font => font.Id == id);

        if (font == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.FontNotFound);
        }
        
        return font;
    }
}