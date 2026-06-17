using Shouldly;
using System.Collections.ObjectModel;
using System.Linq;
using UraniumUI.Dialogs;
using UraniumUI.Material.Controls;
using UraniumUI.Material.Tests.Mocks;
using UraniumUI.Tests.Core;

namespace UraniumUI.Material.Tests.Controls;

public class MultiplePickerField_Test
{
    public MultiplePickerField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication(builder =>
        {
            builder.Services.AddSingleton<IDialogService, MockDialogService>();
        });
    }

    [Fact]
    public void Initialize_WithSelectedItems()
    {
        var control = AnimationReadyHandler.Prepare(new MultiplePickerField());
        var viewModel = new TestViewModel();
        viewModel.SelectedItems.Add(viewModel.ItemsSource[0]);

        control.BindingContext = viewModel;
        control.ItemsSource = viewModel.ItemsSource;

        control.SetBinding(MultiplePickerField.SelectedItemsProperty, new Binding(nameof(TestViewModel.SelectedItems)));

        // Assert
        control.SelectedItems.Count.ShouldBe(1);
        control.SelectedItems[0].ShouldBe(viewModel.ItemsSource[0]);
    }

    [Fact]
    public void SetSelectedItems_ShouldRefreshLayout()
    {
        var control = AnimationReadyHandler.Prepare(new TestMultiplePickerField());

        control.SelectedItems = new ObservableCollection<object>
        {
            "Option 1"
        };

        control.RefreshChipLayoutCallCount.ShouldBe(1);
    }

    [Fact]
    public void ChangeSelectedItems_ShouldRefreshLayout()
    {
        var selectedItems = new ObservableCollection<object>();
        var control = AnimationReadyHandler.Prepare(new TestMultiplePickerField());
        control.SelectedItems = selectedItems;

        selectedItems.Add("Option 1");

        control.RefreshChipLayoutCallCount.ShouldBe(2);
    }

    [Fact]
    public void IsChipRemoveVisible_ShouldUpdateGeneratedChipVisibility()
    {
        var control = AnimationReadyHandler.Prepare(new TestMultiplePickerField());
        control.SelectedItems = new ObservableCollection<object> { "Option 1" };

        var chip = control.GetSingleChip();
        chip.IsDestroyVisible.ShouldBeTrue();

        control.IsChipRemoveVisible = false;
        chip.IsDestroyVisible.ShouldBeFalse();

        control.IsChipRemoveVisible = true;
        chip.IsDestroyVisible.ShouldBeTrue();
    }

    [Fact]
    public void ChipStyleProperties_ShouldUpdateGeneratedChip()
    {
        var control = AnimationReadyHandler.Prepare(new TestMultiplePickerField());
        control.SelectedItems = new ObservableCollection<object> { "Option 1" };

        control.ChipBackgroundColor = Colors.Red;
        control.ChipTextColor = Colors.White;
        control.ChipDestroyIconColor = Colors.Yellow;

        var chip = control.GetSingleChip();
        chip.BackgroundColor.ShouldBe(Colors.Red);
        chip.TextColor.ShouldBe(Colors.White);
        chip.DestroyIconColor.ShouldBe(Colors.Yellow);
    }

    [Fact]
    public void CheckBoxStyleProperties_ShouldUpdateGeneratedPromptCheckBox()
    {
        var control = AnimationReadyHandler.Prepare(new TestMultiplePickerField
        {
            CheckBoxColor = Colors.Red,
            CheckBoxBorderColor = Colors.Blue,
            CheckBoxTextColor = Colors.Green,
            CheckBoxIconColor = Colors.Yellow,
        });

        var checkBox = control.CreateCheckBoxPromptCheckBoxForTest("Option 1", true);

        checkBox.Text.ShouldBe("Option 1");
        checkBox.CommandParameter.ShouldBe("Option 1");
        checkBox.IsChecked.ShouldBeTrue();
        checkBox.Color.ShouldBe(Colors.Red);
        checkBox.BorderColor.ShouldBe(Colors.Blue);
        checkBox.TextColor.ShouldBe(Colors.Green);
        checkBox.IconColor.ShouldBe(Colors.Yellow);
    }

    private sealed class TestMultiplePickerField : MultiplePickerField
    {
        public int RefreshChipLayoutCallCount { get; private set; }

        public Chip GetSingleChip()
        {
            return chipsHolderLayout.Children.OfType<Chip>().Single();
        }

        public UraniumUI.Material.Controls.CheckBox CreateCheckBoxPromptCheckBoxForTest(object item, bool isChecked)
        {
            return CreateCheckBoxPromptCheckBox(item, isChecked);
        }

        protected override void RefreshChipLayout()
        {
            RefreshChipLayoutCallCount++;
        }
    }

    public class TestViewModel : UraniumBindableObject
    {
        public string[] ItemsSource { get; set; } = new string[] { "Option 1", "Option 2", "Option 3", "Option 4", };

        public ObservableCollection<string> SelectedItems { get; } = new();
    }
}
