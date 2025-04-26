using System;
using System.Collections.Generic;
using System.IO;
using R.Paper_Parser.database;

namespace R.Paper_Parser.backend
{
    public class UserService
    {
        private readonly DatabaseHelper _dbHelper;

        public UserService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _dbHelper = new DatabaseHelper(dbPath);
        }

        public bool RegisterUser(string email, string password)
        {
            return _dbHelper.RegisterUser(email, password);
        }

        public User? LoginUser(string email, string password)
        {
            return _dbHelper.LoginUser(email, password);
        }

        public User? GetUserByEmail(string email)
        {
            return _dbHelper.GetUserByEmail(email);
        }

        public User? GetUserById(int id)
        {
            return _dbHelper.GetUserById(id);
        }
    }
}