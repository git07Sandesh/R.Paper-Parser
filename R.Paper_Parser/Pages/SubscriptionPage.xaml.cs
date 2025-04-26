using Microsoft.Maui.Controls;
using System;

namespace R.Paper_Parser.Pages
{
    public partial class SubscriptionPage : ContentPage
    {
        private User _currentUser;
        private DatabaseHelper _db;

        public SubscriptionPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _db = new DatabaseHelper(dbPath);
        }

        private void OnMonthlyPlanClicked(object sender, EventArgs e)
        {
            DisplayAlert("Subscribed", "You have selected the Monthly Plan.", "OK");
            // TODO: Store subscription in DB or trigger payment
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
