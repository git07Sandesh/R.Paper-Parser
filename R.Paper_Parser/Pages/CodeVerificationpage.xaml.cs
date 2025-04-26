using Microsoft.Maui.Controls;

namespace R.Paper_Parser.Pages
{
    public partial class CodeVerificationPage : ContentPage
    {
        public CodeVerificationPage()
        {
            InitializeComponent();
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            string code = VerificationCodeEntry.Text;

            if (string.IsNullOrEmpty(code) || code.Length < 4)
            {
                await DisplayAlert("Error", "Please enter a valid code", "OK");
                return;
            }

            await Shell.Current.GoToAsync("resetpassword");
        }
    }
}
