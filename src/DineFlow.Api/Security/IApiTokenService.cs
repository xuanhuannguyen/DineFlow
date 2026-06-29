using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.Api.Security;

public interface IApiTokenService
{
    string CreateToken(CurrentUserDto user);
    bool TryReadRole(string token, out UserRole role);
}
