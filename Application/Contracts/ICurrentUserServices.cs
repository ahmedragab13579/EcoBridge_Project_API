
namespace EcoBridge.Application.Contracts;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    GetMeResponseDto GetMe();
}