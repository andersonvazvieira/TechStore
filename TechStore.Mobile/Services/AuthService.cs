namespace TechStore.Mobile.Services;

public class AuthService
{
    public string Token { get; private set; } = string.Empty;

    public int UserId { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    public string UserEmail { get; private set; } = string.Empty;

    public string UserRole { get; private set; } = string.Empty;

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(Token);

    public async Task SetLoginAsync(
        string token,
        int userId,
        string userName,
        string userEmail,
        string userRole)
    {
        Token = token;
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        UserRole = userRole;

        await SecureStorage.Default.SetAsync(
            "auth_token",
            Token);

        await SecureStorage.Default.SetAsync(
            "user_id",
            UserId.ToString());

        await SecureStorage.Default.SetAsync(
            "user_name",
            UserName);

        await SecureStorage.Default.SetAsync(
            "user_email",
            UserEmail);

        await SecureStorage.Default.SetAsync(
            "user_role",
            UserRole);
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            Token =
                await SecureStorage.Default.GetAsync(
                    "auth_token")
                ?? string.Empty;

            var userId =
                await SecureStorage.Default.GetAsync(
                    "user_id");

            UserName =
                await SecureStorage.Default.GetAsync(
                    "user_name")
                ?? string.Empty;

            UserEmail =
                await SecureStorage.Default.GetAsync(
                    "user_email")
                ?? string.Empty;

            UserRole =
                await SecureStorage.Default.GetAsync(
                    "user_role")
                ?? string.Empty;

            int.TryParse(
                userId,
                out var parsedUserId);

            UserId = parsedUserId;

            return IsAuthenticated;
        }
        catch
        {
            ClearSession();

            return false;
        }
    }

    public void Logout()
    {
        ClearSession();

        SecureStorage.Default.Remove(
            "auth_token");

        SecureStorage.Default.Remove(
            "user_id");

        SecureStorage.Default.Remove(
            "user_name");

        SecureStorage.Default.Remove(
            "user_email");

        SecureStorage.Default.Remove(
            "user_role");
    }

    private void ClearSession()
    {
        Token = string.Empty;
        UserId = 0;
        UserName = string.Empty;
        UserEmail = string.Empty;
        UserRole = string.Empty;
    }
}