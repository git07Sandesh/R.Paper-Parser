namespace R.Paper_Parser.backend;

public class AuthMiddleware
{
    private static string _currentUserEmail = string.Empty;

    public static void SetCurrentUser(string email)
    {
        _currentUserEmail = email;
    }

    public static string GetCurrentUser()
    {
        return _currentUserEmail;
    }

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrWhiteSpace(_currentUserEmail);
    }
}