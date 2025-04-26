using Microsoft.Maui.Controls;

namespace R.Paper_Parser.Pages
{
    public partial class LandingPage : ContentPage
    {
        public LandingPage()
        {
            InitializeComponent();
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("login");
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("signup");
        }

        private async void OnGoPremiumClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("subscription");
        }
    }
}
