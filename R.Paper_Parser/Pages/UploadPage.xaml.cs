using Microsoft.Maui.Controls;
using System;
using System.IO;
using Microsoft.Maui.Storage;
using R.Paper_Parser.backend;
using R.Paper_Parser.Pages;

namespace R.Paper_Parser.Pages;

public partial class UploadPage : ContentPage
{
    private User _currentUser;
    private DatabaseHelper _db;
    private FileResult? _pickedFile;

    public UploadPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
        _db = new DatabaseHelper(dbPath);
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("Opening file picker...");

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select a research paper",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.MacCatalyst, new[] { ".pdf", ".docx" } },
                    { DevicePlatform.macOS, new[] { ".pdf", ".docx" } },
                    { DevicePlatform.WinUI, new[] { ".pdf", ".docx" } }
                })
            });

            if (result != null)
            {
                _pickedFile = result;
                FileLabel.Text = $"Selected: {_pickedFile.FileName}";
                GenerateButton.IsEnabled = true;
            }
            else
            {
                FileLabel.Text = "No file selected.";
                GenerateButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"File picker failed: {ex.Message}", "OK");
            FileLabel.Text = "No file selected.";
        }
    }

    private async void OnGenerateSummaryClicked(object sender, EventArgs e)
    {
        if (_pickedFile == null)
        {
            await DisplayAlert("Error", "Please select a file first", "OK");
            return;
        }

        try
        {
            // Load and read file
            using var stream = await _pickedFile.OpenReadAsync();
            var fileService = new FileUploadService();
            var summaryService = new SummaryService();

            string savedPath = fileService.SaveFile(stream, _pickedFile.FileName);
            string fileContent = fileService.ReadTextFromFile(savedPath);

            // Generate AI summary
            string summary = await summaryService.GenerateSummaryAsync(fileContent);

            // Save to DB and navigate
            _db.SaveSummary(_currentUser.Id, _pickedFile.FileName, summary);
            await Navigation.PushAsync(new SummaryPage(summary));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Summary generation failed: {ex.Message}", "OK");
        }
    }
}
