using Microsoft.Maui.Controls;

namespace R.Paper_Parser.Pages
{
    public partial class ForgotPassword : ContentPage
    {
        public ForgotPassword()
        {
            InitializeComponent();
        }

        private async void OnSendCodeClicked(object sender, EventArgs e)
        {
            // After "Send Code" is clicked
            await Shell.Current.GoToAsync("codeverification");
        }

        private async void OnSignInTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("login");
        }
    }
}
