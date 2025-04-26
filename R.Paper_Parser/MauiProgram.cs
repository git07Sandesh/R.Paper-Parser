using Microsoft.Extensions.Logging;
using R.Paper_Parser.Pages;
using R.Paper_Parser.backend;
using R.Paper_Parser.database;
using System.IO;
using System;

namespace R.Paper_Parser;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register database helper as singleton
		string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
		builder.Services.AddSingleton<DatabaseHelper>(_ => new DatabaseHelper(dbPath));

		// Register services
		builder.Services.AddTransient<FileUploadService>();
		builder.Services.AddTransient<SummaryService>();
		builder.Services.AddTransient<UserService>();
		builder.Services.AddTransient<HistoryService>();
		builder.Services.AddTransient<PaymentService>();

		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<SignupPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<ForgotPassword>();
		builder.Services.AddTransient<CodeVerificationPage>();
		builder.Services.AddTransient<ResetPasswordPage>();
		builder.Services.AddTransient<UploadPage>();
		builder.Services.AddTransient<SummaryPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<SubscriptionPage>();

		return builder.Build();
	}
}
