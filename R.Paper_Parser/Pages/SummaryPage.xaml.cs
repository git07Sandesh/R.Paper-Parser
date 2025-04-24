using System;
using System.IO;
using System.Threading.Tasks;

namespace R.Paper_Parser.Pages
{
    public partial class SummaryPage : ContentPage
    {
        private string _summaryText = string.Empty;
        private string _fileName = string.Empty; // Initialize with empty string to fix CS8618 warning
        public bool IsPremium { get; set; }

        public SummaryPage(string summaryText)
        {
            InitializeComponent();
            _summaryText = summaryText;
            SummaryLabel.Text = summaryText;
            
            // Default binding context to this instance for property binding in XAML
            BindingContext = this;
            
            // Premium features will be implemented in Phase 5
            IsPremium = false;
        }
        
        public SummaryPage(string summaryText, string fileName, bool isPremium) : this(summaryText)
        {
            _fileName = fileName;
            // Premium features will be implemented in Phase 5
            IsPremium = false;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                FileNameLabel.Text = $"Summary of: {fileName}";
            }
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            try
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = "Research Paper Summary",
                    Text = _summaryText,
                    Subject = !string.IsNullOrEmpty(_fileName) 
                            ? $"Research Paper Summary - {_fileName}" 
                            : "Research Paper Summary"
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to share summary: {ex.Message}", "OK");
            }
        }
        
        private async void OnCopyClicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(_summaryText);
                await DisplayAlert("Success", "Summary copied to clipboard", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to copy summary: {ex.Message}", "OK");
            }
        }
        
        private async void OnSaveAsPdfClicked(object sender, EventArgs e)
        {
            // Premium features will be implemented in Phase 5
            await DisplayAlert("Coming Soon", "PDF export functionality will be added in Phase 5.", "OK");
        }
    }
}
