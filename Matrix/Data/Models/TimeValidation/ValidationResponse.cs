namespace Matrix.Data.Models.TimeValidation;

public class ValidationResponse
{
    public bool SuccessfullyValidated => ValidationFailures.Count == 0;
    public List<ValidationFailure> ValidationFailures { get; init; }
}