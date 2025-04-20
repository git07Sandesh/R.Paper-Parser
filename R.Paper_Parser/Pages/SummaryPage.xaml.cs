namespace R.Paper_Parser.Pages;

public partial class SummaryPage : ContentPage
{
    public SummaryPage(string summaryText)
    {
        InitializeComponent();
        SummaryLabel.Text = summaryText;
    }
}
