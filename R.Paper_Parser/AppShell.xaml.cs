using Microsoft.Maui.Controls;

namespace R.Paper_Parser
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation by name
           Routing.RegisterRoute("login", typeof(Pages.LoginPage));
Routing.RegisterRoute("signup", typeof(Pages.SignupPage));
Routing.RegisterRoute("dashboard", typeof(Pages.DashboardPage));
Routing.RegisterRoute("forgotpassword", typeof(Pages.ForgotPassword));
Routing.RegisterRoute("codeverification", typeof(Pages.CodeVerificationPage));
Routing.RegisterRoute("resetpassword", typeof(Pages.ResetPasswordPage));
Routing.RegisterRoute("subscription", typeof(Pages.SubscriptionPage));
Routing.RegisterRoute("summary", typeof(Pages.SummaryPage));
Routing.RegisterRoute("upload", typeof(Pages.UploadPage));
Routing.RegisterRoute("history", typeof(Pages.HistoryPage));

        }
    }
}
