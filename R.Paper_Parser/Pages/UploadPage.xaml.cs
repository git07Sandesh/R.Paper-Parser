using Microsoft.Maui.Controls;
using System;
using System.IO;
using Microsoft.Maui.Storage;

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
                { DevicePlatform.macOS, new[] { ".pdf", ".docx" } }, // optional fallback
                { DevicePlatform.WinUI, new[] { ".pdf", ".docx" } }
            })
        });

        if (result != null)
        {
            _pickedFile = result;
            FileLabel.Text = $"Selected: {result.FileName}";
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

        // Simulate summary generation
        string mockSummary = $"This is a mock summary for file:\n\n{_pickedFile.FileName}\n\n[In Phase 4, this will be replaced by LLM API output]";
        _db.SaveSummary(_currentUser.Id, _pickedFile.FileName, mockSummary);
        await Navigation.PushAsync(new SummaryPage(mockSummary));

    }
}
