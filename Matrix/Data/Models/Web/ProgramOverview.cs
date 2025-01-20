using Matrix.Data.Types;

namespace Matrix.Data.Models.Web;

public class ProgramOverview
{
    public MatrixState MatrixState { get; init; }
    public Dictionary<string, string> CurrentVariables { get; init; }
    public MatrixTimer? Timer { get; init; }
    public ClockFace? CurrentClockFaceForNow { get; init; }
    public ClockFace? OverridenClockFace { get; init; }
}