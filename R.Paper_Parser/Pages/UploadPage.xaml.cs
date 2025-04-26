using Microsoft.Maui.Controls;
using System;
using System.IO;
using Microsoft.Maui.Storage;
using R.Paper_Parser.backend;
using R.Paper_Parser.database;

namespace R.Paper_Parser.Pages
{
    public partial class UploadPage : ContentPage
    {
        private User _currentUser;
        private DatabaseHelper _db;
        private FileResult? _pickedFile;
        private readonly FileUploadService _fileUploadService;
        private readonly SummaryService _summaryService;

        public UploadPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _db = new DatabaseHelper(dbPath);
            
            // Initialize services
            _fileUploadService = new FileUploadService();
            _summaryService = new SummaryService(_fileUploadService);
        }

        private async void OnPickFileClicked(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Opening file picker...");

                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.pdf", "pdf", "docx", "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document" } },
                    { DevicePlatform.Android, new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
                    { DevicePlatform.WinUI, new[] { ".pdf", ".doc", ".docx" } },
                    { DevicePlatform.macOS, new[] { "pdf", "docx", "doc" } },
                    { DevicePlatform.MacCatalyst, new[] { "pdf", "docx", "doc" } }
                });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a research paper",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                    // Debug file info
                    Console.WriteLine($"Picked file: {result.FileName}, FullPath: {result.FullPath}, ContentType: {result.ContentType}");
                    
                    if (_fileUploadService.ValidateFile(result))
                    {
                        _pickedFile = result;
                        FileLabel.Text = $"Selected: {result.FileName}";
                        GenerateButton.IsEnabled = true;
                    }
                    else
                    {
                        await DisplayAlert("Invalid File", "Please select a PDF or DOCX file under 10MB.", "OK");
                        FileLabel.Text = "No file selected.";
                        GenerateButton.IsEnabled = false;
                    }
                }
                else
                {
                    FileLabel.Text = "No file selected.";
                    GenerateButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File picker error: {ex}");
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
                // Show loading indicator
                GenerateButton.IsEnabled = false;
                FileLabel.Text = "Processing your document...";

                // Generate summary using Gemini API
                SummaryResult result = await _summaryService.GenerateSummary(_pickedFile, _currentUser.IsPremium);
                
                // Even if the API call failed, if we successfully extracted text,
                // we can still show the paper content to the user
                if (!result.IsSuccess && string.IsNullOrEmpty(result.ExtractedText))
                {
                    // Complete failure - no text extracted and no summary
                    await DisplayAlert("Error", result.ErrorMessage, "OK");
                    return;
                }
                
                // If we have extracted text but failed to get a summary, show a warning
                if (!result.IsSuccess && !string.IsNullOrEmpty(result.ExtractedText))
                {
                    await DisplayAlert("Partial Success", 
                        "The paper content was successfully extracted, but we couldn't generate a summary. " +
                        "You can still view the paper content.", "Continue");
                    
                    // Use the fallback summary from the API service
                    if (string.IsNullOrEmpty(result.Summary))
                    {
                        result.Summary = "Unable to generate a summary. Please view the paper content instead.";
                    }
                }
                
                // Save whatever summary we have in database
                _db.SaveSummary(_currentUser.Id, _pickedFile.FileName, result.Summary);
                
                // Navigate to summary page with all relevant information
                await Navigation.PushAsync(new SummaryPage(
                    result.Summary, 
                    result.ExtractedText,
                    _pickedFile.FileName, 
                    _currentUser.IsPremium
                ));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to process document: {ex.Message}", "OK");
            }
            finally
            {
                // Restore UI state
                FileLabel.Text = _pickedFile != null ? $"Selected: {_pickedFile.FileName}" : "No file selected.";
                GenerateButton.IsEnabled = true;
            }
        }
    }
}
