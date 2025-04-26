using Microsoft.Maui.Controls;
using R.Paper_Parser.Pages;
using R.Paper_Parser.database;
using System.IO;
using System;
namespace R.Paper_Parser;

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
        await Navigation.PushAsync(new UploadPage(_currentUser));
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        // TODO: Navigate to Summary History Page
        await Navigation.PushAsync(new HistoryPage(_currentUser));
    }

    private void OnUpgradeClicked(object sender, EventArgs e)
    {
        // TODO: Navigate to Premium Subscription Page
        DisplayAlert("Coming Soon", "Premium upgrade functionality will be added in Phase 5.", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}
