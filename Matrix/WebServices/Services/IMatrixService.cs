using Matrix.Data.Models;

namespace Matrix.WebServices.Services;

public interface IMatrixService
{
    public Task<Dictionary<string, string>> UpdateVariables();
    public ClockFace AddNewClockFace(ClockFace newClockFace);
    public ClockFace GetClockFace(string name);
    public string DeleteClockFace(string name);
}