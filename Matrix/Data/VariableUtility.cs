using System.Globalization;
using System.Text;
using Matrix.Data.Models;
using Matrix.Data.Models.Web;
using Matrix.Data.Utilities;

namespace Matrix.Data;

public class VariableUtility
{
    public static string ParseTime(DateTime dateTime, bool includeSeconds = true, bool militaryTime = false)
    {
        var stringBuilder = new StringBuilder();

        var currentHour = dateTime.Hour;
        if (!militaryTime)
        {
            if (currentHour > 12)
            {
                currentHour -= 12;
            } 
        }
        
        stringBuilder.Append($"{currentHour}:");
        stringBuilder.Append($"{dateTime.Minute:D2}");

        if (includeSeconds)
        {
            stringBuilder.Append($":{dateTime.Second:D2}");
        }

        if (!militaryTime)
        {
            stringBuilder.Append(dateTime.Hour < 12 ? "am" : "pm");
        }

        return stringBuilder.ToString();
    }

    public static string ParseDate(DateTime dateTime)
    {
        return $"{dateTime.Month:D2}-{dateTime.Day:D2}-{dateTime.Year:D2}";
    }

    public static string ParseTimer(MatrixTimer? timer)
    {
        if (timer == null)
        {
            return "None";
        }

        var status = timer.GetFormattedTimer();

        if (status == MatrixTimer.ScreenOn || status == MatrixTimer.ScreenOff)
        {
            status = MatrixTimer.FinishedTimerText;
        }
        
        return status;
    }
    
    public static Dictionary<string, string> BuildVariableDictionary(WeatherModel? weatherData = null, MatrixTimer? timer = null)
    {
        var variablesDictionary = new Dictionary<string, string>();
        
        var time = DateTime.Now;
        
        var currentHour = time.Hour;
        if (currentHour % 12 == 0)
        {
            currentHour += 12;
        }
        else
        {
            currentHour %= 12;
        }
        
        variablesDictionary.Add(VariableConstants.HourVariable, currentHour.ToString());
        variablesDictionary.Add(VariableConstants.MinuteVariable, time.Minute.ToString("D2"));
        variablesDictionary.Add(VariableConstants.SecondVariable, time.Second.ToString("D2"));
        variablesDictionary.Add(VariableConstants.Hour24Variable, time.Hour.ToString());
        variablesDictionary.Add(VariableConstants.AmPmVariable, time.Hour < 12 ? "am" : "pm");
        variablesDictionary.Add(VariableConstants.TimeFormattedVariable, ParseTime(time, false));
        variablesDictionary.Add(VariableConstants.FormattedDateVariable, ParseDate(time));
        variablesDictionary.Add(VariableConstants.MonthNameVariable, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(time.Month));
        variablesDictionary.Add(VariableConstants.DayNameVariable, time.DayOfWeek.ToString());
        variablesDictionary.Add(VariableConstants.MonthNumVariable, time.Month.ToString());
        variablesDictionary.Add(VariableConstants.MonthDayVariable, time.Day.ToString());
        variablesDictionary.Add(VariableConstants.WeekDayNumVariable, ((int) time.DayOfWeek).ToString());
        variablesDictionary.Add(VariableConstants.YearVariable, time.Year.ToString());
        variablesDictionary.Add(VariableConstants.TimerHourVariable, timer?.Hour.ToString() ?? "-1");
        variablesDictionary.Add(VariableConstants.TimerMinuteVariable, timer?.Minute.ToString() ?? "-1");
        variablesDictionary.Add(VariableConstants.TimerSecondVariable, timer?.Second.ToString() ?? "-1");
        variablesDictionary.Add(VariableConstants.TimerFormattedVariable, ParseTimer(timer));

        if (weatherData != null)
        {
            variablesDictionary.Add(VariableConstants.TempVariable, weatherData.Temp.ToString(CultureInfo.CurrentCulture));
            variablesDictionary.Add(VariableConstants.TempLowVariable, weatherData.TempLow.ToString(CultureInfo.CurrentCulture));
            variablesDictionary.Add(VariableConstants.TempHighVariable, weatherData.TempHigh.ToString(CultureInfo.CurrentCulture));
            variablesDictionary.Add(VariableConstants.TempFeelVariable, weatherData.RealFeel.ToString(CultureInfo.CurrentCulture));
            variablesDictionary.Add(VariableConstants.WindSpeedVariable, weatherData.WindSpeed.ToString("0.0"));
            variablesDictionary.Add(VariableConstants.HumidityVariable, weatherData.Humidity.ToString(CultureInfo.CurrentCulture));
            variablesDictionary.Add(VariableConstants.ForecastCurrentVariable, weatherData.CurrentForecast);
            variablesDictionary.Add(VariableConstants.ForecastCurrentShortVariable, weatherData.CurrentForecastShort);
            variablesDictionary.Add(VariableConstants.ForecastDay, weatherData.DayForecast);
        }
        
        return variablesDictionary;
    }
}