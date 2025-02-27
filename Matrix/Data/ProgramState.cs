using Matrix.Data.Models;
using Matrix.Data.Types;
using Matrix.Data.Models.Web;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix.Data;

public class ProgramState
{
    public static MatrixState State { get; set; } = MatrixState.Clock;
    public static MatrixState PreviousState { get; set; } = MatrixState.Clock;
    public static bool UpdateNextTick { get; set; } = true;

    public static bool OverrideClockFace { get; set; } = false;

    public static Dictionary<string, string> CurrentVariables { get; internal set; }

    public static MatrixTimer? Timer { get; set; }

    public static PlainText? PlainText { get; set; }
    
    public static ScrollingText? ScrollingText { get; set; }
    
    public static Image<Rgb24>? Image { get; set; }

    public static WeatherModel? Weather { get; set; }

    public static void RestorePreviousState(MatrixState currentState)
    {
        if (PreviousState != currentState)
        {
            State = PreviousState;
        }
        else
        {
            Console.WriteLine("--- REPAIRING BROKEN STATE BY RESETTING TO CLOCK ---");
            
            MatrixMain.Integrations.BuzzerSensor?.EnsureOff();
            State = MatrixState.Clock;
        }
        
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

        if (State == MatrixState.Text && PlainText != null && (PlainText!.ShouldUpdateSecondly || now.Second == 0))
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