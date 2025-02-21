using Matrix.Data.Types;

namespace Matrix.Data.Models.Web;

public class ProgramOverview
{
    public MatrixState MatrixState { get; init; }
    public MatrixInformation? MatrixInformation { get; init; }
    public int UpdateInterval { get; init; }
    public Dictionary<string, string>? CurrentVariables { get; init; }
    public MatrixTimer? Timer { get; init; }
    public PlainText? PlainText { get; init; }
    public ScrollingText? ScrollingText { get; init; }
    public ClockFace? CurrentClockFace { get; init; }
    public ClockFace? OverridenClockFace { get; init; }
}