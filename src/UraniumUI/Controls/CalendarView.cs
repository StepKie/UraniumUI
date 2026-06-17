using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using UraniumUI.Extensions;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Controls;

public class CalendarView : ContentView
{
    private const int DaysInWeek = 7;
    private const int VisibleWeeks = 6;
    private const int VisibleDayCount = DaysInWeek * VisibleWeeks;
    private const int YearsInPage = 12;
    private const int YearGridColumns = 4;
    private const int YearGridRows = 3;
    private const double MinDayButtonSize = 30;
    private const double MaxDayButtonSize = 40;
    private const uint TransitionOutLength = 80;
    private const uint TransitionInLength = 120;
    private const double TransitionOffset = 18;

    private readonly Label monthLabel = new()
    {
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        HorizontalTextAlignment = TextAlignment.Center,
        FontAttributes = FontAttributes.Bold,
        StyleClass = new[] { "CalendarView.MonthLabel" }
    };

    private readonly StatefulContentView previousMonthButton = CreateNavigationButton(UraniumShapes.ChevronLeft, "CalendarView.PreviousMonthButton", "Previous month");

    private readonly StatefulContentView nextMonthButton = CreateNavigationButton(UraniumShapes.ChevronRight, "CalendarView.NextMonthButton", "Next month");

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

    private readonly Grid yearGrid = new()
    {
        ColumnSpacing = 4,
        RowSpacing = 4,
        IsVisible = false,
        StyleClass = new[] { "CalendarView.YearGrid" }
    };

    private readonly List<Label> weekdayLabels = new(DaysInWeek);
    private readonly List<Button> dayButtons = new(VisibleDayCount);
    private readonly List<Button> yearButtons = new(YearsInPage);
    private IReadOnlyList<CalendarDay> visibleDates = Array.Empty<CalendarDay>();
    private bool isYearSelectionVisible;
    private bool isTransitioning;
    private int yearPageStart;
    private INotifyCollectionChanged selectedDatesNotifier;

    public event EventHandler<CalendarDateSelectedEventArgs> DateSelected;

    public CalendarView()
    {
        StyleClass = new[] { "CalendarView" };

        PreviousMonthCommand = new Command(NavigatePrevious, CanNavigatePrevious);
        NextMonthCommand = new Command(NavigateNext, CanNavigateNext);

        previousMonthButton.TappedCommand = PreviousMonthCommand;
        nextMonthButton.TappedCommand = NextMonthCommand;
        monthLabel.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(ToggleYearSelection) });
        SemanticProperties.SetDescription(monthLabel, "Change year");

        BuildLayout();
        OnSelectedDatesChanged(null, SelectedDates);
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

    public CalendarSelectionMode SelectionMode
    {
        get => (CalendarSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode), typeof(CalendarSelectionMode), typeof(CalendarView), CalendarSelectionMode.Single,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public IList<DateTime> SelectedDates
    {
        get => (IList<DateTime>)GetValue(SelectedDatesProperty);
        set => SetValue(SelectedDatesProperty, value);
    }

    public static readonly BindableProperty SelectedDatesProperty = BindableProperty.Create(
        nameof(SelectedDates), typeof(IList<DateTime>), typeof(CalendarView), defaultBindingMode: BindingMode.TwoWay,
        defaultValueCreator: _ => new ObservableCollection<DateTime>(),
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).OnSelectedDatesChanged((IList<DateTime>)oldValue, (IList<DateTime>)newValue));

    public DateTime? RangeStartDate
    {
        get => (DateTime?)GetValue(RangeStartDateProperty);
        set => SetValue(RangeStartDateProperty, value?.Date);
    }

    public static readonly BindableProperty RangeStartDateProperty = BindableProperty.Create(
        nameof(RangeStartDate), typeof(DateTime?), typeof(CalendarView), default(DateTime?), BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

    public DateTime? RangeEndDate
    {
        get => (DateTime?)GetValue(RangeEndDateProperty);
        set => SetValue(RangeEndDateProperty, value?.Date);
    }

    public static readonly BindableProperty RangeEndDateProperty = BindableProperty.Create(
        nameof(RangeEndDate), typeof(DateTime?), typeof(CalendarView), default(DateTime?), BindingMode.TwoWay,
        propertyChanged: (bindable, oldValue, newValue) => ((CalendarView)bindable).UpdateCalendar());

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
        ApplySelection(date);

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
        SetSelectedDates(Array.Empty<DateTime>());
        RangeStartDate = null;
        RangeEndDate = null;
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

    protected virtual void OnSelectedDatesChanged(IList<DateTime> oldValue, IList<DateTime> newValue)
    {
        if (selectedDatesNotifier is not null)
        {
            selectedDatesNotifier.CollectionChanged -= OnSelectedDatesCollectionChanged;
            selectedDatesNotifier = null;
        }

        selectedDatesNotifier = newValue as INotifyCollectionChanged;

        if (selectedDatesNotifier is not null)
        {
            selectedDatesNotifier.CollectionChanged += OnSelectedDatesCollectionChanged;
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

        for (var column = 0; column < YearGridColumns; column++)
        {
            yearGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (var row = 0; row < YearGridRows; row++)
        {
            yearGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (var index = 0; index < VisibleDayCount; index++)
        {
            var button = new Button
            {
                Padding = 0,
                WidthRequest = MaxDayButtonSize,
                HeightRequest = MaxDayButtonSize,
                CornerRadius = (int)(MaxDayButtonSize / 2),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
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

        for (var index = 0; index < YearsInPage; index++)
        {
            var button = new Button
            {
                Padding = 0,
                MinimumHeightRequest = 40,
                StyleClass = new[] { "CalendarView.YearButton" },
                Command = new Command<int>(SelectYear)
            };

            yearButtons.Add(button);
            yearGrid.Add(button, column: index % YearGridColumns, row: index / YearGridColumns);
        }

        Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                headerGrid,
                weekdayGrid,
                daysGrid,
                yearGrid
            }
        };
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        UpdateDayButtonSize(width);
    }

    protected virtual void UpdateCalendar()
    {
        if (yearPageStart == 0)
        {
            yearPageStart = GetYearPageStart(DisplayDate.Year);
        }

        weekdayGrid.IsVisible = !isYearSelectionVisible;
        daysGrid.IsVisible = !isYearSelectionVisible;
        yearGrid.IsVisible = isYearSelectionVisible;

        if (isYearSelectionVisible)
        {
            monthLabel.Text = $"{yearPageStart} - {Math.Min(yearPageStart + YearsInPage - 1, 9999)}";
            UpdateYearButtons();
        }
        else
        {
            monthLabel.Text = DisplayDate.ToString("Y", CultureInfo.CurrentCulture);
            UpdateWeekdayLabels();

            VisibleDates = BuildVisibleDates();

            for (var index = 0; index < dayButtons.Count; index++)
            {
                UpdateDayButton(dayButtons[index], VisibleDates[index]);
            }
        }

        UpdateNavigationButtonState(previousMonthButton, CanNavigatePrevious());
        UpdateNavigationButtonState(nextMonthButton, CanNavigateNext());

        if (PreviousMonthCommand is Command previousCommand)
        {
            previousCommand.ChangeCanExecute();
        }

        if (NextMonthCommand is Command nextCommand)
        {
            nextCommand.ChangeCanExecute();
        }
    }

    protected IReadOnlyList<CalendarDay> BuildVisibleDates()
    {
        var firstOfMonth = new DateTime(DisplayDate.Year, DisplayDate.Month, 1);
        var offset = ((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek + DaysInWeek) % DaysInWeek;
        var firstVisibleDate = firstOfMonth.AddDays(-offset);
        var days = new List<CalendarDay>(VisibleDayCount);
        var selectedDates = GetSelectedDates();
        var (rangeStart, rangeEnd) = GetSelectedRange();

        for (var i = 0; i < VisibleDayCount; i++)
        {
            var date = firstVisibleDate.AddDays(i);
            var isSingleSelected = SelectionMode == CalendarSelectionMode.Single && SelectedDate?.Date == date;
            var isMultipleSelected = SelectionMode == CalendarSelectionMode.Multiple && selectedDates.Contains(date);
            var isRangeStart = SelectionMode == CalendarSelectionMode.Range && rangeStart == date;
            var isRangeMiddle = SelectionMode == CalendarSelectionMode.Range && rangeStart.HasValue && rangeEnd.HasValue && date > rangeStart.Value && date < rangeEnd.Value;
            var isRangeEnd = SelectionMode == CalendarSelectionMode.Range && rangeEnd == date;

            days.Add(new CalendarDay(
                date,
                IsSameMonth(DisplayDate, date),
                IsDateEnabled(date),
                isSingleSelected || isMultipleSelected || isRangeStart || isRangeMiddle || isRangeEnd,
                isMultipleSelected,
                isRangeStart,
                isRangeMiddle,
                isRangeEnd));
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

        if (day.IsMultipleSelected)
        {
            styleClasses.Add("CalendarView.DayButton.MultipleSelected");
        }

        if (day.IsRangeStart)
        {
            styleClasses.Add("CalendarView.DayButton.RangeStart");
        }

        if (day.IsRangeMiddle)
        {
            styleClasses.Add("CalendarView.DayButton.RangeMiddle");
        }

        if (day.IsRangeEnd)
        {
            styleClasses.Add("CalendarView.DayButton.RangeEnd");
        }

        button.StyleClass = styleClasses.ToArray();
    }

    private void OnSelectedDatesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedDates));
        UpdateCalendar();
    }

    private void ApplySelection(DateTime date)
    {
        switch (SelectionMode)
        {
            case CalendarSelectionMode.Multiple:
                ToggleSelectedDate(date);
                break;
            case CalendarSelectionMode.Range:
                SelectRangeDate(date);
                break;
            default:
                SelectedDate = date;
                break;
        }
    }

    private void ToggleSelectedDate(DateTime date)
    {
        var selectedDates = GetSelectedDates().ToList();

        if (!selectedDates.Remove(date))
        {
            selectedDates.Add(date);
        }

        SetSelectedDates(selectedDates);
    }

    private void SetSelectedDates(IEnumerable<DateTime> dates)
    {
        var normalizedDates = dates.Select(date => date.Date).Distinct().OrderBy(date => date).ToList();

        if (SelectedDates is null || SelectedDates.IsReadOnly)
        {
            SelectedDates = new ObservableCollection<DateTime>(normalizedDates);
            return;
        }

        SelectedDates.Clear();

        foreach (var date in normalizedDates)
        {
            SelectedDates.Add(date);
        }

        OnPropertyChanged(nameof(SelectedDates));
        UpdateCalendar();
    }

    private void SelectRangeDate(DateTime date)
    {
        if (!RangeStartDate.HasValue || RangeEndDate.HasValue)
        {
            RangeStartDate = date;
            RangeEndDate = null;
            return;
        }

        if (date < RangeStartDate.Value.Date)
        {
            RangeEndDate = RangeStartDate.Value.Date;
            RangeStartDate = date;
            return;
        }

        RangeEndDate = date;
    }

    private HashSet<DateTime> GetSelectedDates()
    {
        return SelectedDates?.Select(date => date.Date).ToHashSet() ?? new HashSet<DateTime>();
    }

    private (DateTime? Start, DateTime? End) GetSelectedRange()
    {
        var start = RangeStartDate?.Date;
        var end = RangeEndDate?.Date;

        if (start.HasValue && end.HasValue && end.Value < start.Value)
        {
            return (end.Value, start.Value);
        }

        return (start, end);
    }

    private void UpdateDayButtonSize(double width)
    {
        if (width <= 0)
        {
            return;
        }

        var availableWidth = width - (daysGrid.ColumnSpacing * (DaysInWeek - 1));
        var size = Math.Clamp(Math.Floor(availableWidth / DaysInWeek), MinDayButtonSize, MaxDayButtonSize);

        foreach (var button in dayButtons)
        {
            if (Math.Abs(button.WidthRequest - size) < .1)
            {
                continue;
            }

            button.WidthRequest = size;
            button.HeightRequest = size;
            button.CornerRadius = (int)(size / 2);
        }
    }

    private async void ToggleYearSelection()
    {
        var offset = isYearSelectionVisible ? -TransitionOffset : TransitionOffset;

        await AnimateCalendarUpdateAsync(() =>
        {
            isYearSelectionVisible = !isYearSelectionVisible;
            yearPageStart = GetYearPageStart(DisplayDate.Year);
            UpdateCalendar();
        }, offset);
    }

    private async void NavigatePrevious()
    {
        if (isYearSelectionVisible)
        {
            await AnimateCalendarUpdateAsync(() =>
            {
                yearPageStart = Math.Max(1, yearPageStart - YearsInPage);
                UpdateCalendar();
            }, -TransitionOffset);
            return;
        }

        await AnimateCalendarUpdateAsync(() => DisplayDate = DisplayDate.AddMonths(-1), -TransitionOffset);
    }

    private async void NavigateNext()
    {
        if (isYearSelectionVisible)
        {
            await AnimateCalendarUpdateAsync(() =>
            {
                yearPageStart = Math.Min(GetYearPageStart(9999), yearPageStart + YearsInPage);
                UpdateCalendar();
            }, TransitionOffset);
            return;
        }

        await AnimateCalendarUpdateAsync(() => DisplayDate = DisplayDate.AddMonths(1), TransitionOffset);
    }

    private bool CanNavigatePrevious()
    {
        return isYearSelectionVisible
            ? CanNavigateToYearPage(yearPageStart - YearsInPage)
            : CanNavigateToMonth(DisplayDate.AddMonths(-1));
    }

    private bool CanNavigateNext()
    {
        return isYearSelectionVisible
            ? CanNavigateToYearPage(yearPageStart + YearsInPage)
            : CanNavigateToMonth(DisplayDate.AddMonths(1));
    }

    private async void SelectYear(int year)
    {
        if (!CanNavigateToYear(year))
        {
            return;
        }

        await AnimateCalendarUpdateAsync(() =>
        {
            isYearSelectionVisible = false;
            DisplayDate = GetDisplayDateForYear(year);
            UpdateCalendar();
        }, -TransitionOffset);
    }

    private async Task AnimateCalendarUpdateAsync(Action update, double incomingOffset)
    {
        if (isTransitioning)
        {
            return;
        }

        if (!IsLoaded)
        {
            update();
            return;
        }

        isTransitioning = true;
        var outgoingGrid = GetCurrentSelectionGrid();

        try
        {
            await Task.WhenAll(
                outgoingGrid.FadeToSafely(0, TransitionOutLength, Easing.CubicInOut),
                outgoingGrid.TranslateToSafely(-incomingOffset, 0, TransitionOutLength, Easing.CubicInOut));

            update();

            var incomingGrid = GetCurrentSelectionGrid();
            incomingGrid.Opacity = 0;
            incomingGrid.TranslationX = incomingOffset;

            await Task.WhenAll(
                incomingGrid.FadeToSafely(1, TransitionInLength, Easing.CubicInOut),
                incomingGrid.TranslateToSafely(0, 0, TransitionInLength, Easing.CubicInOut));

            if (outgoingGrid != incomingGrid)
            {
                outgoingGrid.Opacity = 1;
                outgoingGrid.TranslationX = 0;
            }
        }
        finally
        {
            // Ensure we don't leave the grid faded/translated if update/animation fails.
            outgoingGrid.Opacity = 1;
            outgoingGrid.TranslationX = 0;
            isTransitioning = false;
        }
    }

    private Grid GetCurrentSelectionGrid()
    {
        return isYearSelectionVisible ? yearGrid : daysGrid;
    }

    private void UpdateYearButtons()
    {
        for (var index = 0; index < yearButtons.Count; index++)
        {
            UpdateYearButton(yearButtons[index], yearPageStart + index);
        }
    }

    private void UpdateYearButton(Button button, int year)
    {
        var isValidYear = year <= 9999;
        var isEnabled = isValidYear && CanNavigateToYear(year);
        var isSelected = year == DisplayDate.Year;

        button.IsVisible = isValidYear;
        button.Text = isValidYear ? year.ToString(CultureInfo.CurrentCulture) : string.Empty;
        button.CommandParameter = year;
        button.IsEnabled = isEnabled;
        button.Opacity = isEnabled ? 1 : .25;
        button.BackgroundColor = isSelected ? ColorResource.GetColor("Primary", "PrimaryDark", Colors.DodgerBlue) : Colors.Transparent;
        button.TextColor = isSelected
            ? ColorResource.GetColor("OnPrimary", "OnPrimaryDark", Colors.White)
            : ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.Black);

        var styleClasses = new List<string> { "CalendarView.YearButton" };

        if (!isEnabled)
        {
            styleClasses.Add("CalendarView.YearButton.Disabled");
        }

        if (isSelected)
        {
            styleClasses.Add("CalendarView.YearButton.Selected");
        }

        button.StyleClass = styleClasses.ToArray();
    }

    private DateTime GetDisplayDateForYear(int year)
    {
        var day = Math.Min(DisplayDate.Day, DateTime.DaysInMonth(year, DisplayDate.Month));
        var date = new DateTime(year, DisplayDate.Month, day);

        if (MinimumDate.HasValue && date < MinimumDate.Value.Date)
        {
            return MinimumDate.Value.Date;
        }

        if (MaximumDate.HasValue && date > MaximumDate.Value.Date)
        {
            return MaximumDate.Value.Date;
        }

        return date;
    }

    private static StatefulContentView CreateNavigationButton(Geometry pathData, string styleClass, string description)
    {
        var button = new StatefulContentView
        {
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            StyleClass = new[] { "CalendarView.NavigationButton", styleClass },
            Content = new Path
            {
                Data = pathData,
                Stroke = ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.Black),
                StrokeThickness = 3,
                Aspect = Stretch.Uniform,
                WidthRequest = 24,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        };

        SemanticProperties.SetDescription(button, description);

        return button;
    }

    private static void UpdateNavigationButtonState(StatefulContentView button, bool isEnabled)
    {
        button.IsEnabled = isEnabled;
        button.Opacity = isEnabled ? 1 : .35;
    }

    private bool CanNavigateToMonth(DateTime date)
    {
        var firstOfMonth = new DateTime(date.Year, date.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return (!MinimumDate.HasValue || lastOfMonth >= MinimumDate.Value.Date)
            && (!MaximumDate.HasValue || firstOfMonth <= MaximumDate.Value.Date);
    }

    private bool CanNavigateToYearPage(int startYear)
    {
        if (startYear < 1 || startYear > 9999)
        {
            return false;
        }

        var endYear = Math.Min(startYear + YearsInPage - 1, 9999);
        var firstDate = new DateTime(startYear, 1, 1);
        var lastDate = new DateTime(endYear, 12, 31);

        return (!MinimumDate.HasValue || lastDate >= MinimumDate.Value.Date)
            && (!MaximumDate.HasValue || firstDate <= MaximumDate.Value.Date);
    }

    private bool CanNavigateToYear(int year)
    {
        if (year < 1 || year > 9999)
        {
            return false;
        }

        var firstDate = new DateTime(year, 1, 1);
        var lastDate = new DateTime(year, 12, 31);

        return (!MinimumDate.HasValue || lastDate >= MinimumDate.Value.Date)
            && (!MaximumDate.HasValue || firstDate <= MaximumDate.Value.Date);
    }

    private static int GetYearPageStart(int year)
    {
        var pageIndex = (year - 1) / YearsInPage;

        return (pageIndex * YearsInPage) + 1;
    }

    private static bool IsSameMonth(DateTime first, DateTime second)
    {
        return first.Year == second.Year && first.Month == second.Month;
    }
}

public class CalendarDay
{
    public CalendarDay(
        DateTime date,
        bool isCurrentMonth,
        bool isEnabled,
        bool isSelected,
        bool isMultipleSelected = false,
        bool isRangeStart = false,
        bool isRangeMiddle = false,
        bool isRangeEnd = false)
    {
        Date = date.Date;
        IsCurrentMonth = isCurrentMonth;
        IsEnabled = isEnabled;
        IsSelected = isSelected;
        IsMultipleSelected = isMultipleSelected;
        IsRangeStart = isRangeStart;
        IsRangeMiddle = isRangeMiddle;
        IsRangeEnd = isRangeEnd;
    }

    public DateTime Date { get; }

    public bool IsCurrentMonth { get; }

    public bool IsEnabled { get; }

    public bool IsSelected { get; }

    public bool IsMultipleSelected { get; }

    public bool IsRangeStart { get; }

    public bool IsRangeMiddle { get; }

    public bool IsRangeEnd { get; }
}

public enum CalendarSelectionMode
{
    Single,
    Multiple,
    Range
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
