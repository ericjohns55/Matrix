using System.Text;

namespace Matrix.Data;

public class VariableUtility
{
    public static string ParseTime(DateTime dateTime, bool militaryTime = false)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{dateTime.Hour % (militaryTime ? 1 : 12)}:");
        stringBuilder.Append($"{dateTime.Minute:D2}:");
        stringBuilder.Append($"{dateTime.Second:D2}");

        if (!militaryTime)
        {
            stringBuilder.Append(dateTime.Hour > 11 ? " PM" : " AM");
        }

        return stringBuilder.ToString();
    }
}