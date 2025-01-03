namespace Matrix.Data.Models.Web;

public class MatrixResponse<T>
{
    public T Data { get; init; }
    public long ElapsedMilliseconds { get; init; }
}