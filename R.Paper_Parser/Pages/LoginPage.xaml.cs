using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
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

            var user = _databaseHelper.LoginUser(email, password);
            if (user != null)
            {
                await DisplayAlert("Success", "Login successful", "OK");

                // Pass just the user ID and email as string parameters instead of the whole object
                var navigationParameter = new Dictionary<string, object>
                {
                    { "UserId", user.Id.ToString() },
                    { "UserEmail", user.Email }
                };
                await Shell.Current.GoToAsync("dashboard", navigationParameter);
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
