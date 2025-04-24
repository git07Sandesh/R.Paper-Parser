using System;
using Microsoft.Maui.Controls;
using System.IO;
using R.Paper_Parser.database;
using R.Paper_Parser.Pages;

namespace R.Paper_Parser
{
    public partial class MainPage : ContentPage
    {
        private DatabaseHelper _databaseHelper;

        public MainPage()
        {
            InitializeComponent();
            
           string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
    Console.WriteLine($"Database path: {dbPath}");  // show db location in console for debugging
    _databaseHelper = new DatabaseHelper(dbPath);
        }

        // signup func - checks inputs and registers user
        private void OnRegisterClicked(object sender, EventArgs e)
        {
            string email = RegisterEmailEntry.Text;
            string password = RegisterPasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                DisplayAlert("Error", "Email and password cannot be empty", "OK");
                return;
            }

            bool isRegistered = _databaseHelper.RegisterUser(email, password);
            if (isRegistered)
            {
                DisplayAlert("Success", "Registration successful", "OK");
            }
            else
            {
                DisplayAlert("Error", "User already exists", "OK");
            }
        }

        // login func - checks creds and forwards to dashboard if ok
        private void OnLoginClicked(object sender, EventArgs e)
        {
            string email = LoginEmailEntry.Text;
            string password = LoginPasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                DisplayAlert("Error", "Email and password cannot be empty", "OK");
                return;
            }

            bool isLoggedIn = _databaseHelper.LoginUser(email, password);
            if (isLoggedIn)
            {
                DisplayAlert("Success", "Login successful", "OK");
                var user = _databaseHelper.GetUserByEmail(email);  // get user obj
                Navigation.PushAsync(new DashboardPage(user));     // pass to dashboard
            }
            else
            {
                DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }
    }
}

