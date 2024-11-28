namespace Matrix.WebServices;

public class WebException : Exception
{
    public int StatusCode { get; }

    public WebException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}