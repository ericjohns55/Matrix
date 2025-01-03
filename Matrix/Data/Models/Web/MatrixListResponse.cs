namespace Matrix.Data.Models.Web;

public class MatrixListResponse<T> : MatrixResponse<T>
{
    public new List<T> Data { get; init; }
    public int Count => Data.Count;
}