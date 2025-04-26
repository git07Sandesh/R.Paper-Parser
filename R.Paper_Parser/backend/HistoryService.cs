using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using R.Paper_Parser.database;

namespace R.Paper_Parser.backend
{
    public class HistoryService
    {
        private readonly DatabaseHelper _dbHelper;

        public HistoryService(string dbPath)
        {
            _dbHelper = new DatabaseHelper(dbPath);
        }

        // Get all summaries for a user - isPremium parameter will be used in Phase 5
        public List<Summary> GetUserSummaryHistory(int userId, bool isPremium)
        {
            return _dbHelper.GetUserSummaries(userId, isPremium);
        }

        // Delete a specific summary by ID
        public bool DeleteSummary(int summaryId, int userId)
        {
            return _dbHelper.DeleteSummary(summaryId, userId);
        }

        // Search through user's summaries - isPremium parameter will be used in Phase 5
        public List<Summary> SearchSummaries(int userId, string searchTerm, bool isPremium)
        {
            return _dbHelper.SearchSummaries(userId, searchTerm, isPremium);
        }
    }
}