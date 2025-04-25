using System.Collections.Generic;

namespace R.Paper_Parser.backend;

public class UserService
{
    private readonly Dictionary<string, string> _roles = new();

    public void SetUserRole(string email, string role)
    {
        _roles[email] = role;
    }

    public string GetUserRole(string email)
    {
        return _roles.TryGetValue(email, out var role) ? role : "basic";
    }
}