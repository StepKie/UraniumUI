using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;

namespace UraniumUI.Material.Tests.Controls;
public class DatePickerField_Test
{
    public DatePickerField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void Date_BindingForInitialization_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel { Date = DateTime.Now.AddDays(2) };
        control.BindingContext = viewModel;
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(TestViewModel.Date)));

        // Assert
        control.Date.ShouldBe(viewModel.Date);
    }

    [Fact]
    public void Date_Binding_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel { Date = DateTime.Now.AddDays(2) };
        control.BindingContext = viewModel;
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(TestViewModel.Date)));

        // Act
        viewModel.Date = DateTime.Parse("09:05");

        // Assert
        control.Date.ShouldBe(viewModel.Date);
    }

    [Fact]
    public void Date_Binding_ToSource()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel { Date = DateTime.Now.AddDays(2) };
        control.BindingContext = viewModel;
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(TestViewModel.Date)));

        // Act
        control.Date = DateTime.Parse("09:05");

        // Assert
        viewModel.Date.ShouldBe(control.Date);
    }

    [Fact]
    public void Date_Binding_ShouldSupportNonNullableDateTime()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new NonNullableDateViewModel { Date = DateTime.Now.AddDays(2) };
        control.BindingContext = viewModel;
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(NonNullableDateViewModel.Date)));

        // Assert source to control binding.
        control.Date.ShouldBe(viewModel.Date);

        // Act
        control.Date = DateTime.Parse("09:05");

        // Assert control to source binding.
        viewModel.Date.ShouldBe(control.Date.Value);
    }

    [Fact]
    public void Date_Binding_ShouldAcceptNull_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel { Date = null };
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(TestViewModel.Date)));

        // Assert
        control.Date.ShouldBeNull();
        control.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Date_Binding_ShouldAcceptNull_ToSource()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel { Date = DateTime.Now.AddDays(2) };
        control.BindingContext = viewModel;
        control.SetBinding(DatePickerField.DateProperty, new Binding(nameof(TestViewModel.Date)));

        // Act
        control.Date = null;

        // Assert
        viewModel.Date.ShouldBeNull();
        control.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Clear_ShouldSetDateToNull()
    {
        var control = AnimationReadyHandler.Prepare(new TestDatePickerField { Date = DateTime.Today });

        // Act
        control.Clear();

        // Assert
        control.Date.ShouldBeNull();
        control.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void DatePickerView_DateSelected_ShouldUpdateDate()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var date = DateTime.Today.AddDays(2);

        // Act
        control.DatePickerView.Date = date;

        // Assert
        control.Date.ShouldBe(date);
    }

    [Fact]
    public void MaximumDate_ShouldUseDatePickerDefault_WhenSetToNull()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());

        // Act
        control.MaximumDate = DateTime.Today.AddDays(1);
        control.MaximumDate = null;

        // Assert
        control.DatePickerView.MaximumDate.ShouldBe((DateTime)DatePicker.MaximumDateProperty.DefaultValue);
    }

    [Fact]
    public void MinimumDate_ShouldUseDatePickerDefault_WhenSetToNull()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());

        // Act
        control.MinimumDate = DateTime.Today.AddDays(-1);
        control.MinimumDate = null;

        // Assert
        control.DatePickerView.MinimumDate.ShouldBe((DateTime)DatePicker.MinimumDateProperty.DefaultValue);
    }

    [Fact]
    public void Format_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.Format = "HH:mm"; //24H format
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.FormatProperty, new Binding(nameof(TestViewModel.Format)));

        // Assert
        control.DatePickerView.Format.ShouldBe(viewModel.Format);
    }

    [Fact]
    public void TextColor_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.TextColor = Colors.Blue;
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.TextColorProperty, new Binding(nameof(TestViewModel.TextColor)));

        // Assert
        control.DatePickerView.TextColor.ShouldBe(viewModel.TextColor);
    }

    [Fact]
    public void CharacterSpacing_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.CharacterSpacing = 4.41;
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.CharacterSpacingProperty, new Binding(nameof(TestViewModel.CharacterSpacing)));

        // Assert
        control.DatePickerView.CharacterSpacing.ShouldBe(viewModel.CharacterSpacing);
    }

    [Fact]
    public void FontAttributes_ShouldBeSet_FromControl()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var fontAttributes = FontAttributes.Italic;

        // Act
        control.FontAttributes = fontAttributes;

        // Assert
        control.DatePickerView.FontAttributes.ShouldBe(fontAttributes);
    }

    [Fact]
    public void FontAttributes_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.FontAttributes = FontAttributes.Italic;
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.FontAttributesProperty, new Binding(nameof(TestViewModel.FontAttributes)));

        // Assert
        control.DatePickerView.FontAttributes.ShouldBe(viewModel.FontAttributes);
    }

    [Fact]
    public void FontFamily_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.FontFamily = "Roboto";
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.FontFamilyProperty, new Binding(nameof(TestViewModel.FontFamily)));

        // Assert
        control.DatePickerView.FontFamily.ShouldBe(viewModel.FontFamily);
    }

    [Fact]
    public void FontFamily_ShouldBeSet_FromControl()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var fontFamily = "Roboto";

        // Act
        control.FontFamily = fontFamily;

        // Assert
        control.DatePickerView.FontFamily.ShouldBe(fontFamily);
    }

    [Fact]
    public void FontSize_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var viewModel = new TestViewModel();
        viewModel.FontSize = 28.5;
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(DatePickerField.FontSizeProperty, new Binding(nameof(TestViewModel.FontSize)));

        // Assert
        control.DatePickerView.FontSize.ShouldBe(viewModel.FontSize);
    }

    [Fact]
    public void FontSize_ShouldBeSet_FromControl()
    {
        var control = AnimationReadyHandler.Prepare(new DatePickerField());
        var fontSize = 24.75;

        // Act
        control.FontSize = fontSize;

        // Assert
        control.DatePickerView.FontSize.ShouldBe(fontSize);
    }

    public class TestViewModel : UraniumBindableObject
    {
        private DateTime? time;
        private string format;
        private Color textColor;
        private double characterSpacing;
        private FontAttributes fontAttributes;
        private string fontFamily;
        private double fontSize;

        public DateTime? Date { get => time; set => SetProperty(ref time, value); }

        public string Format { get => format; set => SetProperty(ref format, value); }

        public Color TextColor { get => textColor; set => SetProperty(ref textColor, value); }

        public double CharacterSpacing { get => characterSpacing; set => SetProperty(ref characterSpacing, value); }

        public FontAttributes FontAttributes { get => fontAttributes; set => SetProperty(ref fontAttributes, value); }

        public string FontFamily { get => fontFamily; set => SetProperty(ref fontFamily, value); }

        public double FontSize { get => fontSize; set => SetProperty(ref fontSize, value); }
    }

    private class TestDatePickerField : DatePickerField
    {
        public void Clear()
        {
            OnClearTapped(this);
        }
    }

    private class NonNullableDateViewModel : UraniumBindableObject
    {
        private DateTime date;

        public DateTime Date { get => date; set => SetProperty(ref date, value); }
    }
}
