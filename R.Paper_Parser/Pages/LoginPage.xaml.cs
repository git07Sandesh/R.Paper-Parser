using Microsoft.Maui.Controls;
using System;
using System.IO;
using R.Paper_Parser.database;

namespace R.Paper_Parser.Pages
{
    public partial class LoginPage : ContentPage
    {
        private DatabaseHelper _databaseHelper;

        public LoginPage()
        {
            InitializeComponent();

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _databaseHelper = new DatabaseHelper(dbPath);
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            string email = LoginEmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Email and password cannot be empty", "OK");
                return;
            }

            bool isLoggedIn = _databaseHelper.LoginUser(email, password);
            if (isLoggedIn)
            {
                await DisplayAlert("Success", "Login successful", "OK");

                await Shell.Current.GoToAsync("dashboard");
            }
            else
            {
                await DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("forgotpassword");
        }

        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("signup");
        }
    }
}
