namespace Unify.Erp.Application.Auth;

public enum AuthenticationError
{
    None = 0,
    InvalidCredentials = 1,
    DisabledUser = 2,
    InvalidRefreshToken = 3,
    ExpiredRefreshToken = 4,
    ReusedRefreshToken = 5
}
