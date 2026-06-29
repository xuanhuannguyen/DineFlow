using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Auth;

public class CurrentUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}
