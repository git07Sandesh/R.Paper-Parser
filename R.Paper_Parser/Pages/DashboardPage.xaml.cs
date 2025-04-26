using Microsoft.Maui.Controls;
using R.Paper_Parser.database;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace R.Paper_Parser.Pages
{
    [QueryProperty(nameof(UserId), "UserId")]
    [QueryProperty(nameof(UserEmail), "UserEmail")]
    public partial class DashboardPage : ContentPage
    {
        private int _userId;
        private string? _userEmail;
        private DatabaseHelper _db;

        public string UserId
        {
            set
            {
                if (int.TryParse(value, out int id))
                {
                    _userId = id;
                    LoadUserData();
                }
            }
        }

        public string UserEmail
        {
            set
            {
                _userEmail = value;
                UpdateWelcomeMessage();
            }
        }

        public DashboardPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _db = new DatabaseHelper(dbPath);
        }

        private void LoadUserData()
        {
            // Additional user data can be loaded here
        }

        private void UpdateWelcomeMessage()
        {
            if (!string.IsNullOrEmpty(_userEmail))
            {
                welcomeLabel.Text = $"Welcome, {_userEmail}";
            }
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "UserId", _userId.ToString() }
            };
            await Shell.Current.GoToAsync("upload", navigationParameter);
        }

        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "UserId", _userId.ToString() }
            };
            await Shell.Current.GoToAsync("history", navigationParameter);
        }

        private async void OnUpgradeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("subscription");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }
}
