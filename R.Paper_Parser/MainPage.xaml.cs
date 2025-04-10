
using System;
using Microsoft.Maui.Controls;
using System.IO;

namespace R.Paper_Parser;

public partial class MainPage : ContentPage
{
	private DatabaseHelper _databaseHelper;

        public MainPage()
        {
            InitializeComponent();
            
           string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
    Console.WriteLine($"Database path: {dbPath}");  // Log the database path to verify
    _databaseHelper = new DatabaseHelper(dbPath);
        }

        // Handle Register Button Click
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

        // Handle Login Button Click
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
                // Redirect to next page or dashboard here
            }
            else
            {
                DisplayAlert("Error", "Invalid credentials", "OK");
            }
        }

	
}

