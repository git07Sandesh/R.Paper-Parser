using System;
using System.Collections.Generic;
using System.IO;
using R.Paper_Parser.database;
using R.Paper_Parser.backend;

namespace R.Paper_Parser.Pages
{
    public partial class HistoryPage : ContentPage
    {
        private User _user;
        private HistoryService _historyService;
        private List<Summary> _summaries = new List<Summary>(); // Initialize with empty list

        public HistoryPage(User user)
        {
            InitializeComponent();
            _user = user;
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _historyService = new HistoryService(dbPath);

            // Premium features will be implemented in Phase 5
            // Hide premium UI elements for now
            SearchLayout.IsVisible = false;
            BasicUserLabel.IsVisible = false;

            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                // isPremium parameter will be relevant in Phase 5
                _summaries = _historyService.GetUserSummaryHistory(_user.Id, _user.IsPremium);
                HistoryList.ItemsSource = _summaries;
                
                // Display message if no summaries found
                EmptyStateLabel.IsVisible = _summaries.Count == 0;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
            }
        }
        
        // Search functionality (will be expanded in Phase 5 with premium features)
        public void SearchSummaries(string searchTerm)
        {
            _summaries = _historyService.SearchSummaries(_user.Id, searchTerm, _user.IsPremium);
            HistoryList.ItemsSource = _summaries;
            EmptyStateLabel.IsVisible = _summaries.Count == 0;
        }
        
        // Handle item tapped to view full summary
        private async void OnSummaryTapped(object sender, EventArgs e)
        {
            // Updated to use Border instead of Frame (Frame is obsolete in .NET 9)
            if (sender is Border border && border.BindingContext is Summary summary)
            {
                // For now, all users get the basic summary view
                // Premium features will be added in Phase 5
                await Navigation.PushAsync(new SummaryPage(summary.SummaryText, summary.FileName, false));
            }
        }
        
        // Handle search button click - will be used in Phase 5 with premium features
        private void OnSearchClicked(object sender, EventArgs e)
        {
            string searchTerm = SearchEntry.Text?.Trim() ?? string.Empty;
            SearchSummaries(searchTerm);
        }
    }
}
