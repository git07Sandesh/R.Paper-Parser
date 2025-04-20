namespace R.Paper_Parser.Pages;

public partial class HistoryPage : ContentPage
{
    private User _user;
    private DatabaseHelper _db;

    public HistoryPage(User user)
    {
        InitializeComponent();
        _user = user;
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "userdata.db3");
        _db = new DatabaseHelper(dbPath);

        LoadHistory();
    }

    private void LoadHistory()
    {
        var summaries = _db.GetUserSummaries(_user.Id, _user.IsPremium);
        HistoryList.ItemsSource = summaries;
    }
}
