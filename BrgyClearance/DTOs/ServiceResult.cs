namespace Proj1.DTOs;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public static ServiceResult Success(string? message = null)
    {
        return new ServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static ServiceResult Failure(string errorMessage)
    {
        return new ServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }

    public static ServiceResult Failure(IEnumerable<string> errors)
    {
        return new ServiceResult
        {
            Succeeded = false,
            Errors = errors,
            ErrorMessage = string.Join(", ", errors)
        };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, string? message = null)
    {
        return new ServiceResult<T>
        {
            Succeeded = true,
            Data = data,
            SuccessMessage = message
        };
    }

    public new static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }

    public new static ServiceResult<T> Failure(IEnumerable<string> errors)
    {
        return new ServiceResult<T>
        {
            Succeeded = false,
            Errors = errors,
            ErrorMessage = string.Join(", ", errors)
        };
    }
}