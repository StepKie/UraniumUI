using UraniumUI.Controls;

namespace UraniumApp.Pages;

public partial class CalendarViewPage : ContentPage
{
    public CalendarViewPage()
    {
        InitializeComponent();

        calendarView.MinimumDate = DateTime.Today.AddMonths(-2);
        calendarView.MaximumDate = DateTime.Today.AddMonths(2);
        calendarView.DateSelected += (sender, args) => UpdateSelectionSummary();

        UpdateSelectionSummary();
    }

    private void TodayClicked(object sender, EventArgs e)
    {
        calendarView.TrySelectDate(DateTime.Today);
        UpdateSelectionSummary();
    }

    private void ClearClicked(object sender, EventArgs e)
    {
        calendarView.ClearSelection();
        UpdateSelectionSummary();
    }

    private void SingleModeClicked(object sender, EventArgs e)
    {
        calendarView.SelectionMode = CalendarSelectionMode.Single;
        UpdateSelectionSummary();
    }

    private void MultipleModeClicked(object sender, EventArgs e)
    {
        calendarView.SelectionMode = CalendarSelectionMode.Multiple;
        UpdateSelectionSummary();
    }

    private void RangeModeClicked(object sender, EventArgs e)
    {
        calendarView.SelectionMode = CalendarSelectionMode.Range;
        UpdateSelectionSummary();
    }

    private void UpdateSelectionSummary()
    {
        selectionSummaryLabel.Text = calendarView.SelectionMode switch
        {
            CalendarSelectionMode.Multiple => $"Selected dates: {FormatDates(calendarView.SelectedDates)}",
            CalendarSelectionMode.Range => $"Selected range: {FormatDate(calendarView.RangeStartDate)} - {FormatDate(calendarView.RangeEndDate)}",
            _ => $"Selected date: {FormatDate(calendarView.SelectedDate)}"
        };
    }

    private static string FormatDates(IEnumerable<DateTime> dates)
    {
        var formattedDates = dates.Select(date => date.ToString("yyyy-MM-dd")).ToArray();

        return formattedDates.Length == 0 ? "none" : string.Join(", ", formattedDates);
    }

    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd") ?? "none";
    }
}
