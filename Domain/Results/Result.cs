using EcoBridge.Domain.Errors;

namespace EcoBridge.Domain.Results;

public class Result<T>
{
    public bool Success { get; protected set; }
    public bool IsSuccess => Success;

    public T? Value { get; protected set; }
    public T? Data => Value;

    public string Message { get; protected set; } = AppMessageCodes.SUCCESS;
    public string Code => Message;

    public ErrorType ErrorType { get; protected set; } = ErrorType.None;
    public ErrorType Error => ErrorType;

    // --- Success Factories ---
    public static Result<T> SuccessResult(T data, string code = AppMessageCodes.SUCCESS)
    {
        return new Result<T>
        {
            Success = true,
            Value = data,
            Message = code,
            ErrorType = ErrorType.None
        };
    }

    public static Result<T> Ok(T data, string code = AppMessageCodes.SUCCESS)
    {
        return SuccessResult(data, code);
    }

    // --- Failure Factories with Specific Error Types ---
    public static Result<T> FailResult(T data, string code, ErrorType errorType = ErrorType.Failure)
    {
        return new Result<T>
        {
            Success = false,
            Value = data,
            Message = code,
            ErrorType = errorType
        };
    }

    public static Result<T> Fail(string code, ErrorType errorType = ErrorType.Failure, T? data = default)
    {
        return new Result<T>
        {
            Success = false,
            Value = data,
            Message = code,
            ErrorType = errorType
        };
    }

    // --- Explicit Semantic Result Creators (Handling all specific cases) ---
    public static Result<T> NotFound(string code = AppMessageCodes.NOT_FOUND, T? data = default)
    {
        return Fail(code, ErrorType.NotFound, data);
    }

    public static Result<T> Unauthorized(string code = AppMessageCodes.UNAUTHORIZED, T? data = default)
    {
        return Fail(code, ErrorType.Unauthorized, data);
    }

    public static Result<T> Forbidden(string code = AppMessageCodes.FORBIDDEN, T? data = default)
    {
        return Fail(code, ErrorType.Forbidden, data);
    }

    public static Result<T> Conflict(string code = AppMessageCodes.CONFLICT, T? data = default)
    {
        return Fail(code, ErrorType.Conflict, data);
    }

    public static Result<T> Validation(string code = AppMessageCodes.VALIDATION_ERROR, T? data = default)
    {
        return Fail(code, ErrorType.Validation, data);
    }
}

public class Result : Result<object?>
{
    public static Result SuccessResult(string code = AppMessageCodes.SUCCESS)
    {
        return new Result
        {
            Success = true,
            Value = null,
            Message = code,
            ErrorType = ErrorType.None
        };
    }

    public static Result Ok(string code = AppMessageCodes.SUCCESS)
    {
        return SuccessResult(code);
    }

    public static Result Fail(string code, ErrorType errorType = ErrorType.Failure)
    {
        return new Result
        {
            Success = false,
            Value = null,
            Message = code,
            ErrorType = errorType
        };
    }

    public static Result NotFound(string code = AppMessageCodes.NOT_FOUND)
    {
        return Fail(code, ErrorType.NotFound);
    }

    public static Result Unauthorized(string code = AppMessageCodes.UNAUTHORIZED)
    {
        return Fail(code, ErrorType.Unauthorized);
    }

    public static Result Forbidden(string code = AppMessageCodes.FORBIDDEN)
    {
        return Fail(code, ErrorType.Forbidden);
    }

    public static Result Conflict(string code = AppMessageCodes.CONFLICT)
    {
        return Fail(code, ErrorType.Conflict);
    }

    public static Result Validation(string code = AppMessageCodes.VALIDATION_ERROR)
    {
        return Fail(code, ErrorType.Validation);
    }
}
