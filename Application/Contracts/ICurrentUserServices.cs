using EcoBridge.Domain.Results;

namespace EcoBridge.Application.Contracts;

public interface ICurrentUserServices
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    Result<GetMeResponseDto> GetMe();
}
