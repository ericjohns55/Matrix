using Matrix.Data.Models;

namespace Matrix.WebServices.Services;

public interface IMatrixService
{
    public ClockFace AddNewClockFace(ClockFace newClockFace);
    public ClockFace GetClockFace(string name);
    public string DeleteClockFace(string name);
}