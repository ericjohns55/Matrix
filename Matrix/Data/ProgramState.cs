using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Models.Web;

namespace Matrix.Data;

public class ProgramState
{
    public static MatrixState State { get; set; } = MatrixState.Clock;
    public static MatrixState PreviousState { get; set; } = MatrixState.Clock;
    public static bool UpdateNextTick { get; set; } = true;

    public static bool OverrideClockFace { get; set; } = false;

    public static Dictionary<string, string> CurrentVariables { get; internal set; }

    public static MatrixTimer? Timer { get; set; }

    public static WeatherModel? Weather { get; set; }

    public static void RestorePreviousState(MatrixState currentState)
    {
        State = PreviousState;
        PreviousState = currentState;
        UpdateNextTick = true;
    }

    public static bool NeedsUpdate(DateTime now, ClockFace? currentClockFace)
    {
        if (UpdateNextTick)
        {
            return true;
        }

        if (State == MatrixState.Clock)
        {
            if (now.Second == 0 || (currentClockFace != null && currentClockFace.UpdatesEverySecond))
            {
                return true;
            }
        }

        if (State == MatrixState.Timer && Timer != null && Timer.NeedsScreenUpdate())
        {
            return true;
        }

        if (State == MatrixState.ScrollingText)
        {
            return true;
        }

        return false;
    }

    public static void UpdateVariables()
    {
        CurrentVariables = VariableUtility.BuildVariableDictionary(Weather, Timer);
    }
}