namespace Matrix.Data.Models.Web;

public class ExceptionResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; }
    public string Details { get; init; }
}