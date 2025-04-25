using System;
using System.IO;
using System.Threading.Tasks;

namespace R.Paper_Parser.Pages
{
    public partial class SummaryPage : ContentPage
    {
        private string _summaryText = string.Empty;
        private string _extractedText = string.Empty;
        private string _fileName = string.Empty;
        private bool _currentlyShowingSummary = true;
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
        
        public SummaryPage(string summaryText, string extractedText, string fileName, bool isPremium)
        {
            InitializeComponent();
            
            _summaryText = summaryText;
            _extractedText = extractedText;
            _fileName = fileName;
            IsPremium = isPremium;
            
            // Set the text for both views
            SummaryLabel.Text = _summaryText;
            FullTextLabel.Text = _extractedText;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                FileNameLabel.Text = $"Analysis of: {fileName}";
            }
            
            // Default binding context to this instance for property binding in XAML
            BindingContext = this;
            
            // Default tab is summary view
            UpdateTabDisplay(true);
        }

        private void UpdateTabDisplay(bool showingSummary)
        {
            _currentlyShowingSummary = showingSummary;
            
            // Update UI based on which tab is active
            SummaryView.IsVisible = showingSummary;
            FullTextView.IsVisible = !showingSummary;
            
            // Style the buttons to highlight the active tab
            SummaryTabButton.BackgroundColor = showingSummary ? Color.FromArgb("#512BD4") : Color.FromArgb("#7744E7");
            FullTextTabButton.BackgroundColor = !showingSummary ? Color.FromArgb("#512BD4") : Color.FromArgb("#7744E7");
        }
        
        private void OnSummaryTabClicked(object sender, EventArgs e)
        {
            UpdateTabDisplay(true);
        }
        
        private void OnFullTextTabClicked(object sender, EventArgs e)
        {
            UpdateTabDisplay(false);
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            try
            {
                // Share either summary or full text based on current tab
                string textToShare = _currentlyShowingSummary ? _summaryText : _extractedText;
                string title = _currentlyShowingSummary ? "Research Paper Summary" : "Research Paper Content";
                
                await Share.RequestAsync(new ShareTextRequest
                {
                    Title = title,
                    Text = textToShare,
                    Subject = !string.IsNullOrEmpty(_fileName) 
                            ? $"{title} - {_fileName}" 
                            : title
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to share: {ex.Message}", "OK");
            }
        }
        
        private async void OnCopyClicked(object sender, EventArgs e)
        {
            try
            {
                // Copy either summary or full text based on current tab
                string textToCopy = _currentlyShowingSummary ? _summaryText : _extractedText;
                string contentType = _currentlyShowingSummary ? "Summary" : "Paper content";
                
                await Clipboard.SetTextAsync(textToCopy);
                await DisplayAlert("Success", $"{contentType} copied to clipboard", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to copy: {ex.Message}", "OK");
            }
        }
        
        private async void OnSaveAsPdfClicked(object sender, EventArgs e)
        {
            if (!IsPremium)
            {
                await DisplayAlert("Premium Feature", "PDF export is available for premium users only. Upgrade your account to access this feature.", "OK");
                return;
            }
            
            // Premium PDF export functionality
            await DisplayAlert("Coming Soon", "PDF export functionality will be added in a future update.", "OK");
        }
    }
}
