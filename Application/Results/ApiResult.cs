using EcoBridge.Domain.Errors;

namespace EcoBridge.Application.Result;

public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public string Code { get; set; } = AppMessageCodes.SUCCESS;
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T data, string code = AppMessageCodes.SUCCESS)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Code = code,
            Data = data
        };
    }

    public static ApiResult<T> Fail(string errorCode, T? data = default)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Code = errorCode,
            Data = data
        };
    }
}

public class ApiResult : ApiResult<object>
{
    public static ApiResult Ok(string code = AppMessageCodes.SUCCESS)
    {
        return new ApiResult
        {
            IsSuccess = true,
            Code = code,
            Data = null
        };
    }

    public static ApiResult Fail(string errorCode)
    {
        return new ApiResult
        {
            IsSuccess = false,
            Code = errorCode,
            Data = null
        };
    }
}
