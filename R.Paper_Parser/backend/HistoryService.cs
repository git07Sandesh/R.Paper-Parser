using System.Collections.Generic;

namespace R.Paper_Parser.backend;

public class HistoryService
{
    private readonly Dictionary<string, List<string>> _userHistories = new();

    public void AddSummary(string email, string summary)
    {
        if (!_userHistories.ContainsKey(email))
            _userHistories[email] = new List<string>();

        _userHistories[email].Add(summary);
    }

    public List<string> GetHistory(string email)
    {
        return _userHistories.ContainsKey(email) ? _userHistories[email] : new List<string>();
    }
}