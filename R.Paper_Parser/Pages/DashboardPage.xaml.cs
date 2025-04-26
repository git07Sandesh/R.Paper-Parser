using Microsoft.Maui.Controls;
using R.Paper_Parser.database;
using System.IO;
using System;
using System.Threading.Tasks;

namespace R.Paper_Parser.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private User _currentUser;
        private DatabaseHelper _db;

        public DashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _db = new DatabaseHelper(dbPath);
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            // Option 1: if you want to pass user object directly
            await Shell.Current.GoToAsync("upload");
            // (we will later show how to pass parameters properly if needed)
        }

        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("history");
        }

        private async void OnUpgradeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("subscription");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Send the user back to login page when logout
            await Shell.Current.GoToAsync("//login");
        }
    }
}
