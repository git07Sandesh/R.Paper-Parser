using System;
using System.Collections.Generic;
using System.IO;
using R.Paper_Parser.database;

namespace R.Paper_Parser.backend
{
    public class PaymentService
    {
        private readonly DatabaseHelper _dbHelper;

        public PaymentService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _dbHelper = new DatabaseHelper(dbPath);
        }

        public bool UpgradeToPremium(int userId)
        {
            var user = _dbHelper.GetUserById(userId);
            if (user != null)
            {
                user.IsPremium = true;
                return true;
            }
            
            return false;
        }

        public bool IsUserPremium(int userId)
        {
            var user = _dbHelper.GetUserById(userId);
            return user?.IsPremium ?? false;
        }
    }
}