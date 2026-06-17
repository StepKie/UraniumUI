using System.Collections.ObjectModel;
using UraniumUI.Controls;
using UraniumUI.Extensions;
using UraniumUI.Tests.Core;
using UraniumUI.Views;

namespace UraniumUI.Tests.Controls;

public class CalendarView_Test
{
    public CalendarView_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void VisibleDates_ShouldBuildSixWeekGrid_WhenMonthStartsOnFirstDayOfWeek()
    {
        var calendarView = new CalendarView
        {
            FirstDayOfWeek = DayOfWeek.Monday,
            DisplayDate = new DateTime(2026, 6, 15)
        };

        Assert.Equal(42, calendarView.VisibleDates.Count);
        Assert.Equal(new DateTime(2026, 6, 1), calendarView.VisibleDates.First().Date);
        Assert.Equal(new DateTime(2026, 7, 12), calendarView.VisibleDates.Last().Date);
    }

    [Fact]
    public void VisibleDates_ShouldIncludeLeadingDays_WhenMonthDoesNotStartOnFirstDayOfWeek()
    {
        var calendarView = new CalendarView
        {
            FirstDayOfWeek = DayOfWeek.Monday,
            DisplayDate = new DateTime(2026, 5, 15)
        };

        Assert.Equal(new DateTime(2026, 4, 27), calendarView.VisibleDates.First().Date);
        Assert.Equal(new DateTime(2026, 5, 1), calendarView.VisibleDates[4].Date);
        Assert.False(calendarView.VisibleDates[0].IsCurrentMonth);
        Assert.True(calendarView.VisibleDates[4].IsCurrentMonth);
    }

    [Fact]
    public void TrySelectDate_ShouldSetSelectedDate_WhenDateIsEnabled()
    {
        var calendarView = new CalendarView();
        var date = DateTime.Today.AddDays(1);

        var selected = calendarView.TrySelectDate(date);

        Assert.True(selected);
        Assert.Equal(date.Date, calendarView.SelectedDate);
    }

    [Fact]
    public void TrySelectDate_ShouldRaiseDateSelected_WhenSameDateIsSelectedAgain()
    {
        var date = DateTime.Today;
        var calendarView = new CalendarView
        {
            SelectedDate = date
        };
        var eventCount = 0;

        calendarView.DateSelected += (sender, args) =>
        {
            eventCount++;
            Assert.Equal(date.Date, args.OldDate);
            Assert.Equal(date.Date, args.SelectedDate);
        };

        var selected = calendarView.TrySelectDate(date);

        Assert.True(selected);
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void TrySelectDate_ShouldIgnoreDate_WhenDateIsBeforeMinimumDate()
    {
        var calendarView = new CalendarView
        {
            MinimumDate = new DateTime(2026, 5, 10),
            MaximumDate = new DateTime(2026, 5, 20)
        };

        var selected = calendarView.TrySelectDate(new DateTime(2026, 5, 9));

        Assert.False(selected);
        Assert.Null(calendarView.SelectedDate);
    }

    [Fact]
    public void TrySelectDate_ShouldIgnoreDate_WhenDateIsAfterMaximumDate()
    {
        var calendarView = new CalendarView
        {
            MinimumDate = new DateTime(2026, 5, 10),
            MaximumDate = new DateTime(2026, 5, 20)
        };

        var selected = calendarView.TrySelectDate(new DateTime(2026, 5, 21));

        Assert.False(selected);
        Assert.Null(calendarView.SelectedDate);
    }

    [Fact]
    public void ClearSelection_ShouldSetSelectedDateToNull()
    {
        var calendarView = new CalendarView
        {
            SelectedDate = DateTime.Today
        };

        calendarView.ClearSelection();

        Assert.Null(calendarView.SelectedDate);
    }

    [Fact]
    public void TrySelectDate_ShouldToggleSelectedDates_WhenSelectionModeIsMultiple()
    {
        var firstDate = new DateTime(2026, 5, 10);
        var secondDate = new DateTime(2026, 5, 12);
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Multiple,
            DisplayDate = new DateTime(2026, 5, 1)
        };

        Assert.True(calendarView.TrySelectDate(firstDate));
        Assert.True(calendarView.TrySelectDate(secondDate));

        Assert.Equal(new[] { firstDate, secondDate }, calendarView.SelectedDates);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == firstDate).IsMultipleSelected);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == firstDate).IsSelected);

        Assert.True(calendarView.TrySelectDate(firstDate));

        Assert.Equal(new[] { secondDate }, calendarView.SelectedDates);
        Assert.False(calendarView.VisibleDates.Single(day => day.Date == firstDate).IsSelected);
    }

    [Fact]
    public void VisibleDates_ShouldUpdate_WhenSelectedDatesCollectionChanges()
    {
        var selectedDates = new ObservableCollection<DateTime>();
        var date = new DateTime(2026, 5, 10);
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Multiple,
            DisplayDate = new DateTime(2026, 5, 1),
            SelectedDates = selectedDates
        };

        selectedDates.Add(date);

        Assert.True(calendarView.VisibleDates.Single(day => day.Date == date).IsMultipleSelected);
    }

    [Fact]
    public void TrySelectDate_ShouldSelectRange_WhenSelectionModeIsRange()
    {
        var startDate = new DateTime(2026, 5, 10);
        var middleDate = new DateTime(2026, 5, 12);
        var endDate = new DateTime(2026, 5, 15);
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Range,
            DisplayDate = new DateTime(2026, 5, 1)
        };

        Assert.True(calendarView.TrySelectDate(startDate));

        Assert.Equal(startDate, calendarView.RangeStartDate);
        Assert.Null(calendarView.RangeEndDate);

        Assert.True(calendarView.TrySelectDate(endDate));

        Assert.Equal(startDate, calendarView.RangeStartDate);
        Assert.Equal(endDate, calendarView.RangeEndDate);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == startDate).IsRangeStart);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == middleDate).IsRangeMiddle);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == endDate).IsRangeEnd);
    }

    [Fact]
    public void TrySelectDate_ShouldNormalizeRange_WhenEndIsBeforeStart()
    {
        var firstTap = new DateTime(2026, 5, 15);
        var secondTap = new DateTime(2026, 5, 10);
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Range,
            DisplayDate = new DateTime(2026, 5, 1)
        };

        Assert.True(calendarView.TrySelectDate(firstTap));
        Assert.True(calendarView.TrySelectDate(secondTap));

        Assert.Equal(secondTap, calendarView.RangeStartDate);
        Assert.Equal(firstTap, calendarView.RangeEndDate);
    }

    [Fact]
    public void TrySelectDate_ShouldAllowSameDateRange()
    {
        var date = new DateTime(2026, 5, 10);
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Range,
            DisplayDate = new DateTime(2026, 5, 1)
        };

        Assert.True(calendarView.TrySelectDate(date));
        Assert.True(calendarView.TrySelectDate(date));

        var day = calendarView.VisibleDates.Single(day => day.Date == date);

        Assert.Equal(date, calendarView.RangeStartDate);
        Assert.Equal(date, calendarView.RangeEndDate);
        Assert.True(day.IsRangeStart);
        Assert.True(day.IsRangeEnd);
        Assert.True(day.IsSelected);
    }

    [Fact]
    public void TrySelectDate_ShouldStartNewRange_WhenRangeIsComplete()
    {
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Range
        };

        Assert.True(calendarView.TrySelectDate(new DateTime(2026, 5, 10)));
        Assert.True(calendarView.TrySelectDate(new DateTime(2026, 5, 15)));
        Assert.True(calendarView.TrySelectDate(new DateTime(2026, 5, 20)));

        Assert.Equal(new DateTime(2026, 5, 20), calendarView.RangeStartDate);
        Assert.Null(calendarView.RangeEndDate);
    }

    [Fact]
    public void TrySelectDate_ShouldIgnoreDisabledDate_WhenSelectionModeIsRange()
    {
        var calendarView = new CalendarView
        {
            SelectionMode = CalendarSelectionMode.Range,
            MinimumDate = new DateTime(2026, 5, 10),
            MaximumDate = new DateTime(2026, 5, 20)
        };

        var selected = calendarView.TrySelectDate(new DateTime(2026, 5, 9));

        Assert.False(selected);
        Assert.Null(calendarView.RangeStartDate);
        Assert.Null(calendarView.RangeEndDate);
    }

    [Fact]
    public void ClearSelection_ShouldClearAllSelectionProperties()
    {
        var calendarView = new CalendarView
        {
            SelectedDate = new DateTime(2026, 5, 9),
            RangeStartDate = new DateTime(2026, 5, 10),
            RangeEndDate = new DateTime(2026, 5, 15)
        };
        calendarView.SelectedDates.Add(new DateTime(2026, 5, 11));

        calendarView.ClearSelection();

        Assert.Null(calendarView.SelectedDate);
        Assert.Empty(calendarView.SelectedDates);
        Assert.Null(calendarView.RangeStartDate);
        Assert.Null(calendarView.RangeEndDate);
    }

    [Fact]
    public void NextMonthCommand_ShouldAdvanceDisplayDate()
    {
        var calendarView = new CalendarView
        {
            DisplayDate = new DateTime(2026, 5, 15)
        };

        calendarView.NextMonthCommand.Execute(null);

        Assert.Equal(new DateTime(2026, 6, 15), calendarView.DisplayDate);
    }

    [Fact]
    public void PreviousMonthCommand_ShouldNotNavigate_WhenPreviousMonthIsBeforeMinimumDate()
    {
        var calendarView = new CalendarView
        {
            DisplayDate = new DateTime(2026, 5, 15),
            MinimumDate = new DateTime(2026, 5, 10)
        };

        Assert.False(calendarView.PreviousMonthCommand.CanExecute(null));
    }

    [Fact]
    public void VisibleDates_ShouldMarkDatesOutsideMinMaxAsDisabled()
    {
        var calendarView = new CalendarView
        {
            FirstDayOfWeek = DayOfWeek.Monday,
            DisplayDate = new DateTime(2026, 5, 15),
            MinimumDate = new DateTime(2026, 5, 10),
            MaximumDate = new DateTime(2026, 5, 20)
        };

        Assert.False(calendarView.VisibleDates.Single(day => day.Date == new DateTime(2026, 5, 9)).IsEnabled);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == new DateTime(2026, 5, 10)).IsEnabled);
        Assert.True(calendarView.VisibleDates.Single(day => day.Date == new DateTime(2026, 5, 20)).IsEnabled);
        Assert.False(calendarView.VisibleDates.Single(day => day.Date == new DateTime(2026, 5, 21)).IsEnabled);
    }

    [Fact]
    public void MonthYearToggle_ShouldBeFocusableAndExposeSemanticText()
    {
        var calendarView = new CalendarView();

        var monthButton = calendarView.FindManyInChildrenHierarchy<StatefulContentView>()
            .Distinct()
            .Single(button => SemanticProperties.GetDescription(button) == "Change year");

        Assert.True(monthButton.IsFocusable);
        Assert.NotNull(monthButton.TappedCommand);
        Assert.Equal("Shows year selection.", SemanticProperties.GetHint(monthButton));
    }
}
