public class Result
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = new();

    public static Result Success() => new Result { IsSuccess = true };

    public static Result Failure(List<string> errors) =>
        new Result { IsSuccess = false, Errors = errors };
    public static Result Failure(string error) =>
        new Result
        {
            IsSuccess = false,
            Errors = new List<string> { error }
        };
    public string ErrorMessage => string.Join("; ", Errors);
}