using System.Globalization;
using System.Windows.Input;
using UraniumUI.Resources;

namespace UraniumUI.Controls;

public class CalendarView : ContentView
{
    private const int DaysInWeek = 7;
    private const int VisibleWeeks = 6;
    private const int VisibleDayCount = DaysInWeek * VisibleWeeks;

    private readonly Label monthLabel = new()
    {
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        HorizontalTextAlignment = TextAlignment.Center,
        FontAttributes = FontAttributes.Bold,
        StyleClass = new[] { "CalendarView.MonthLabel" }
    };

    private readonly Button previousMonthButton = new()
    {
        Text = "<",
        StyleClass = new[] { "CalendarView.NavigationButton", "CalendarView.PreviousMonthButton" }
    };

    private readonly Button nextMonthButton = new()
    {
        Text = ">",
        StyleClass = new[] { "CalendarView.NavigationButton", "CalendarView.NextMonthButton" }
    };

    private readonly Grid weekdayGrid = new()
    {
        ColumnSpacing = 4,
        StyleClass = new[] { "CalendarView.WeekdayGrid" }
    };

    private readonly Grid daysGrid = new()
    {
        ColumnSpacing = 4,
        RowSpacing = 4,
        StyleClass = new[] { "CalendarView.DaysGrid" }
    };

    private readonly List<Label> weekdayLabels = new(DaysInWeek);
    private readonly List<Button> dayButtons = new(VisibleDayCount);
    private IReadOnlyList<CalendarDay> visibleDates = Array.Empty<CalendarDay>();

    public event EventHandler<CalendarDateSelectedEventArgs> DateSelected;

    public CalendarView()
    {
        StyleClass = new[] { "CalendarView" };

        PreviousMonthCommand = new Command(() => DisplayDate = DisplayDate.AddMonths(-1), () => CanNavigateToMonth(DisplayDate.AddMonths(-1)));
        NextMonthCommand = new Command(() => DisplayDate = DisplayDate.AddMonths(1), () => CanNavigateToMonth(DisplayDate.AddMonths(1)));

        previousMonthButton.Command = PreviousMonthCommand;
        nextMonthButton.Command = NextMonthCommand;

        BuildLayout();
        UpdateCalendar();
    }

    public IReadOnlyList<CalendarDay> VisibleDates
    {
        get => visibleDates;
        private set
        {
            visibleDates = value;
            OnPropertyChanged();
        }
    }

    public ICommand PreviousMonthCommand { get; }

    public ICommand NextMonthCommand { get; }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value?.Date);
    }

    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
        nameof(SelectedDate), typeof(DateTime?), typeof(CalendarView), default(DateTime?), BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).OnSelectedDateChanged((DateTime?)newValue));

    public DateTime DisplayDate
    {
        get => (DateTime)GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value.Date);
    }

    public static readonly BindableProperty DisplayDateProperty = BindableProperty.Create(
        nameof(DisplayDate), typeof(DateTime), typeof(CalendarView), DateTime.Today,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value?.Date);
    }

    public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(
        nameof(MinimumDate), typeof(DateTime?), typeof(CalendarView), default(DateTime?),
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public DateTime? MaximumDate
    {
        get => (DateTime?)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value?.Date);
    }

    public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(
        nameof(MaximumDate), typeof(DateTime?), typeof(CalendarView), default(DateTime?),
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    public static readonly BindableProperty FirstDayOfWeekProperty = BindableProperty.Create(
        nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(CalendarView), CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public bool TrySelectDate(DateTime date)
    {
        date = date.Date;

        if (!IsDateEnabled(date))
        {
            return false;
        }

        var oldDate = SelectedDate;
        SelectedDate = date;

        if (!IsSameMonth(DisplayDate, date))
        {
            DisplayDate = date;
        }

        DateSelected?.Invoke(this, new CalendarDateSelectedEventArgs(oldDate, date));

        return true;
    }

    public void ClearSelection()
    {
        SelectedDate = null;
    }

    protected virtual void OnSelectedDateChanged(DateTime? selectedDate)
    {
        if (selectedDate.HasValue && !IsSameMonth(DisplayDate, selectedDate.Value))
        {
            DisplayDate = selectedDate.Value;
            return;
        }

        UpdateCalendar();
    }

    protected virtual void BuildLayout()
    {
        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            StyleClass = new[] { "CalendarView.Header" }
        };

        headerGrid.Add(previousMonthButton, column: 0);
        headerGrid.Add(monthLabel, column: 1);
        headerGrid.Add(nextMonthButton, column: 2);

        for (var column = 0; column < DaysInWeek; column++)
        {
            weekdayGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var label = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                StyleClass = new[] { "CalendarView.WeekdayLabel" }
            };

            weekdayLabels.Add(label);
            weekdayGrid.Add(label, column);
        }

        for (var column = 0; column < DaysInWeek; column++)
        {
            daysGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (var row = 0; row < VisibleWeeks; row++)
        {
            daysGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (var index = 0; index < VisibleDayCount; index++)
        {
            var button = new Button
            {
                Padding = 0,
                MinimumHeightRequest = 40,
                MinimumWidthRequest = 40,
                CornerRadius = 20,
                StyleClass = new[] { "CalendarView.DayButton" },
                Command = new Command<CalendarDay>(day =>
                {
                    if (day != null)
                    {
                        TrySelectDate(day.Date);
                    }
                })
            };

            dayButtons.Add(button);
            daysGrid.Add(button, column: index % DaysInWeek, row: index / DaysInWeek);
        }

        Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                headerGrid,
                weekdayGrid,
                daysGrid
            }
        };
    }

    protected virtual void UpdateCalendar()
    {
        monthLabel.Text = DisplayDate.ToString("Y", CultureInfo.CurrentCulture);
        UpdateWeekdayLabels();

        VisibleDates = BuildVisibleDates();

        for (var index = 0; index < dayButtons.Count; index++)
        {
            UpdateDayButton(dayButtons[index], VisibleDates[index]);
        }

        previousMonthButton.IsEnabled = CanNavigateToMonth(DisplayDate.AddMonths(-1));
        nextMonthButton.IsEnabled = CanNavigateToMonth(DisplayDate.AddMonths(1));

        if (PreviousMonthCommand is Command previousCommand)
        {
            previousCommand.ChangeCanExecute();
        }

        if (NextMonthCommand is Command nextCommand)
        {
            nextCommand.ChangeCanExecute();
        }
    }

    protected virtual IReadOnlyList<CalendarDay> BuildVisibleDates()
    {
        var firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
        var offset = ((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek + DaysInWeek) % DaysInWeek;
        var firstVisibleDate = firstOfMonth.AddDays(-offset);
        var days = new List<CalendarDay>(VisibleDayCount);

        for (var i = 0; i < VisibleDayCount; i++)
        {
            var date = firstVisibleDate.AddDays(i);
            days.Add(new CalendarDay(
                date,
                IsSameMonth(DisplayDate, date),
                IsDateEnabled(date),
                SelectedDate?.Date == date));
        }

        return days;
    }

    protected virtual bool IsDateEnabled(DateTime date)
    {
        date = date.Date;

        if (MinimumDate.HasValue && date < MinimumDate.Value.Date)
        {
            return false;
        }

        if (MaximumDate.HasValue && date > MaximumDate.Value.Date)
        {
            return false;
        }

        return true;
    }

    private void UpdateWeekdayLabels()
    {
        for (var index = 0; index < DaysInWeek; index++)
        {
            var day = (DayOfWeek)(((int)FirstDayOfWeek + index) % DaysInWeek);
            weekdayLabels[index].Text = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(day);
        }
    }

    private void UpdateDayButton(Button button, CalendarDay day)
    {
        button.Text = day.Date.Day.ToString(CultureInfo.CurrentCulture);
        button.CommandParameter = day;
        button.IsEnabled = day.IsEnabled;
        button.Opacity = day.IsEnabled ? day.IsCurrentMonth ? 1 : .45 : .25;
        button.BackgroundColor = day.IsSelected ? ColorResource.GetColor("Primary", "PrimaryDark", Colors.DodgerBlue) : Colors.Transparent;
        button.TextColor = day.IsSelected
            ? ColorResource.GetColor("OnPrimary", "OnPrimaryDark", Colors.White)
            : ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.Black);

        var styleClasses = new List<string> { "CalendarView.DayButton" };

        if (!day.IsCurrentMonth)
        {
            styleClasses.Add("CalendarView.DayButton.OutsideMonth");
        }

        if (!day.IsEnabled)
        {
            styleClasses.Add("CalendarView.DayButton.Disabled");
        }

        if (day.IsSelected)
        {
            styleClasses.Add("CalendarView.DayButton.Selected");
        }

        button.StyleClass = styleClasses.ToArray();
    }

    private bool CanNavigateToMonth(DateTime date)
    {
        var firstOfMonth = new DateTime(date.Year, date.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return (!MinimumDate.HasValue || lastOfMonth >= MinimumDate.Value.Date)
            && (!MaximumDate.HasValue || firstOfMonth <= MaximumDate.Value.Date);
    }

    private static bool IsSameMonth(DateTime first, DateTime second)
    {
        return first.Year == second.Year && first.Month == second.Month;
    }
}

public class CalendarDay
{
    public CalendarDay(DateTime date, bool isCurrentMonth, bool isEnabled, bool isSelected)
    {
        Date = date.Date;
        IsCurrentMonth = isCurrentMonth;
        IsEnabled = isEnabled;
        IsSelected = isSelected;
    }

    public DateTime Date { get; }

    public bool IsCurrentMonth { get; }

    public bool IsEnabled { get; }

    public bool IsSelected { get; }
}

public class CalendarDateSelectedEventArgs : EventArgs
{
    public CalendarDateSelectedEventArgs(DateTime? oldDate, DateTime selectedDate)
    {
        OldDate = oldDate?.Date;
        SelectedDate = selectedDate.Date;
    }

    public DateTime? OldDate { get; }

    public DateTime SelectedDate { get; }
}
