using System;
using Microsoft.Maui.Controls;
using System.IO;
using R.Paper_Parser.database;

namespace R.Paper_Parser.Pages
{
    public partial class SignupPage : ContentPage
    {
        private DatabaseHelper _databaseHelper;

        public SignupPage()
        {
            InitializeComponent();

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _databaseHelper = new DatabaseHelper(dbPath);
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            string email = RegisterEmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Email and password cannot be empty", "OK");
                return;
            }

            bool isRegistered = _databaseHelper.RegisterUser(email, password);
            if (isRegistered)
            {
                await DisplayAlert("Success", "Registration successful", "OK");

                await Shell.Current.GoToAsync("login");
            }
            else
            {
                await DisplayAlert("Error", "User already exists", "OK");
            }
        }

        private async void OnSignInTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("login");
        }
    }
}
