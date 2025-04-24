using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace R.Paper_Parser.database
{
    public class DatabaseHelper
    {
        private SQLiteConnection _database;

        public DatabaseHelper(string dbPath)
        {
            // Use platform-specific directory for SQLite storage
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _database = new SQLiteConnection(databasePath);
            _database.CreateTable<User>(); 
            _database.CreateTable<Summary>();
        }

        // Register new user
        public bool RegisterUser(string email, string password)
        {
            var userExists = _database.Table<User>().FirstOrDefault(u => u.Email == email);
            if (userExists != null)
                return false;  // User already exists

            var user = new User
            {
                Email = email,
                Password = password  // In a real app, hash the password here
            };
            
            _database.Insert(user);

            // Debug: Check if the user was inserted
            var insertedUser = _database.Table<User>().FirstOrDefault(u => u.Email == email);
            Console.WriteLine($"Inserted User: {insertedUser?.Email}, {insertedUser?.Password}");
        
            return true;
        }
        
        public User GetUserByEmail(string email)
        {
            return _database.Table<User>().FirstOrDefault(u => u.Email == email);
        }

        // Login user
        public bool LoginUser(string email, string password)
        {
            var user = _database.Table<User>().FirstOrDefault(u => u.Email == email);
            if (user == null)
                return false;  // User doesn't exist

            return user.Password == password;  // In real apps, hash the password and compare hashes
        }
        
        public void SaveSummary(int userId, string fileName, string summaryText)
        {
            var summary = new Summary
            {
                UserId = userId,
                FileName = fileName,
                SummaryText = summaryText,
                Timestamp = DateTime.Now
            };
            _database.Insert(summary);
        }
        
        public List<Summary> GetUserSummaries(int userId, bool isPremium)
        {
            var query = _database.Table<Summary>()
                                .Where(s => s.UserId == userId)
                                .OrderByDescending(s => s.Timestamp);

            // In Phase 5, this will be limited based on subscription status
            // For now, all users can see all their summaries
            return query.ToList();
        }
        
        // Delete a specific summary
        public bool DeleteSummary(int summaryId, int userId)
        {
            // First check if summary exists and belongs to the user
            var summary = _database.Table<Summary>()
                                  .FirstOrDefault(s => s.Id == summaryId && s.UserId == userId);
            
            if (summary == null)
                return false;
                
            var result = _database.Delete<Summary>(summaryId);
            return result > 0;
        }
        
        // Search summaries by content or filename
        public List<Summary> SearchSummaries(int userId, string searchTerm, bool isPremium)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetUserSummaries(userId, isPremium);
                
            var query = _database.Table<Summary>()
                                .Where(s => s.UserId == userId && 
                                       (s.FileName.ToLower().Contains(searchTerm.ToLower()) || 
                                        s.SummaryText.ToLower().Contains(searchTerm.ToLower())))
                                .OrderByDescending(s => s.Timestamp);
                                
            // In Phase 5, this will be limited based on subscription status
            // For now, all users can see all their summaries
            return query.ToList();
        }
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        // Premium status will be added in Phase 5
        public bool IsPremium { get; set; } = false;
    }

    public class Summary
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}