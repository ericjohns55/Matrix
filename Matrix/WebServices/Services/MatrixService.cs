using Matrix.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Matrix.WebServices.Services;

public class MatrixService : IMatrixService
{
    private MatrixContext _matrixContext;
    
    public MatrixService(MatrixContext context)
    {
        _matrixContext = context;
    }

    public ClockFace AddNewClockFace(ClockFace newClockFace)
    {
        return new ClockFace();
    }

    public ClockFace GetClockFace(string name)
    {
        return new ClockFace();
    }

    public string DeleteClockFace(string name)
    {
        return string.Empty;
    }
}