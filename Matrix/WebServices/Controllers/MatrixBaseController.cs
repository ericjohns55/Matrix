using System.Diagnostics;
using Matrix.Data.Exceptions;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Utilities;
using Matrix.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Controllers;

public class MatrixBaseController : Controller
{
    public static MatrixResponse<T> ExecuteToMatrixResponse<T>(Func<T> func)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var data = func.Invoke();
        stopwatch.Stop();

        return new MatrixResponse<T>()
        {
            Data = data,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
        };
    }
    
    public static Task<MatrixResponse<T>> ExecuteToMatrixResponseAsync<T>(Func<Task<T>> func)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        return func.Invoke().OnSuccess(result =>
        {
            stopwatch.Start();

            return new MatrixResponse<T>()
            {
                Data = result,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        });
    }
    
    public static Task<MatrixListResponse<T>> ExecuteToMatrixListResponseAsync<T>(Func<Task<List<T>>> func)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        return func.Invoke().OnSuccess(result =>
        {
            stopwatch.Start();

            return new MatrixListResponse<T>()
            {
                Data = result,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        });
    }
    
    protected async Task<MatrixColor> LoadMatrixColorFromId(MatrixContext matrixContext, int id)
    {
        var color = await matrixContext.MatrixColor.FirstOrDefaultAsync(color => color.Id == id);

        if (color == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.ColorNotFound);
        }

        return color;
    }

    protected async Task<MatrixFont> LoadMatrixFontFromId(MatrixContext matrixContext, int id)
    {
        var font = await matrixContext.MatrixFont.FirstOrDefaultAsync(font => font.Id == id);

        if (font == null)
        {
            throw new MatrixEntityNotFoundException(WebConstants.FontNotFound);
        }
        
        return font;
    }
}