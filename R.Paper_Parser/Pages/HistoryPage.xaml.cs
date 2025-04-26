using System;
using System.Collections.Generic;
using System.IO;
using R.Paper_Parser.database;
using R.Paper_Parser.backend;

namespace R.Paper_Parser.Pages
{
    [QueryProperty(nameof(UserId), "UserId")]
    public partial class HistoryPage : ContentPage
    {
        private User? _user;
        private HistoryService _historyService;
        private List<Summary> _summaries = new List<Summary>();

        public HistoryPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
            _historyService = new HistoryService(dbPath);

            SearchLayout.IsVisible = false;
            BasicUserLabel.IsVisible = false;
            EmptyStateLabel.IsVisible = true;
        }

        public string UserId
        {
            set
            {
                if (int.TryParse(value, out int id))
                {
                    LoadUserAndHistory(id);
                }
            }
        }

        private void LoadUserAndHistory(int id)
        {
            _user = _historyService.GetUser(id);
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (_user == null)
                {
                    EmptyStateLabel.IsVisible = true;
                    return;
                }

                _summaries = _historyService.GetUserSummaryHistory(_user.Id, _user.IsPremium);
                HistoryList.ItemsSource = _summaries;
                EmptyStateLabel.IsVisible = _summaries.Count == 0;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
            }
        }
        
        public void SearchSummaries(string searchTerm)
        {
            if (_user == null)
            {
                return;
            }
            
            _summaries = _historyService.SearchSummaries(_user.Id, searchTerm, _user.IsPremium);
            HistoryList.ItemsSource = _summaries;
            EmptyStateLabel.IsVisible = _summaries.Count == 0;
        }
        
        private async void OnSummaryTapped(object sender, EventArgs e)
        {
            if (_user == null)
            {
                return;
            }

            if (sender is Border border && border.BindingContext is Summary summary)
            {
                await Navigation.PushAsync(new SummaryPage(
                    summary.SummaryText, 
                    "Original paper content not available in history view.", 
                    summary.FileName, 
                    _user.IsPremium));
            }
        }
        
        private void OnSearchClicked(object sender, EventArgs e)
        {
            string searchTerm = SearchEntry.Text?.Trim() ?? string.Empty;
            SearchSummaries(searchTerm);
        }
    }
}
