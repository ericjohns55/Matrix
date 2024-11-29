using Matrix.Data.Types;
using Timer = Matrix.Data.Models.Timer;

namespace Matrix.Data;

public class ProgramState
{
    public static MatrixState State { get; set; } = MatrixState.Clock;
    public static MatrixState PreviousState { get; set; } = MatrixState.Unknown;
    public static bool Update { get; set; } = true;
    
    public static Timer Timer { get; set; }

    public static bool NeedsUpdate(DateTime now)
    {
        if (Update)
        {
            return true;
        }

        if (State == MatrixState.Clock && now.Second == 0)
        {
            return true;
        }

        if (State == MatrixState.Timer)
        {
            return true;
        }

        if (State == MatrixState.ScrollingText)
        {
            return true;
        }


        return false;
    }
}