using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;
using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;
using MaterialCheckBox = UraniumUI.Material.Controls.CheckBox;

namespace UraniumUI.Material.Tests.Controls;
public class DataGrid_Test
{
    public DataGrid_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void LineSeparatorColor_BindingForInitialization_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid());
        var viewModel = new { LineSeparatorColor = Colors.Black };
        control.BindingContext = viewModel;
        control.SetBinding(DataGrid.LineSeparatorColorProperty, new Binding(nameof(viewModel.LineSeparatorColor)));

        // Assert
        control.LineSeparatorColor.ShouldBe(viewModel.LineSeparatorColor);
    }

    [Fact]
    public void LineSeparatorColor_ShouldBeUpdated_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid());
        var viewModel = new DataGridTestViewModel { LineSeparatorColor = Colors.Black };
        control.BindingContext = viewModel;
        control.SetBinding(DataGrid.LineSeparatorColorProperty, new Binding(nameof(viewModel.LineSeparatorColor)));

        // Act
        viewModel.LineSeparatorColor = Colors.Red;

        // Assert
        control.LineSeparatorColor.ShouldBe(viewModel.LineSeparatorColor);
    }

    [Fact]
    public void ShowHeaders_ShouldHideHeaderViews_AndMoveFirstRowToTop()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            ShowHeaders = false,
            Columns =
            [
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
                new DataGridColumn { Title = "Name", ValueBinding = new Binding(nameof(DataGridRow.Name)) },
            ],
            ItemsSource = new List<DataGridRow>
            {
                new() { Id = 1, Name = "One" },
                new() { Id = 2, Name = "Two" },
            }
        });

        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        rootGrid.Children.OfType<Label>().Count().ShouldBe(0);
        rootGrid.Children.OfType<ContentView>().Select(Grid.GetRow).Distinct().OrderBy(x => x).ToArray().ShouldBe([0, 2]);
    }

    [Fact]
    public void ShowHeaders_ShouldRerender_WhenUpdated()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            Columns =
            [
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
                new DataGridColumn { Title = "Name", ValueBinding = new Binding(nameof(DataGridRow.Name)) },
            ],
            ItemsSource = new List<DataGridRow>
            {
                new() { Id = 1, Name = "One" },
            }
        });

        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        rootGrid.Children.OfType<Label>().Count().ShouldBe(2);
        rootGrid.Children.OfType<ContentView>().Select(Grid.GetRow).Distinct().Single().ShouldBe(2);

        control.ShowHeaders = false;

        rootGrid = control.Content.ShouldBeOfType<Grid>();
        rootGrid.Children.OfType<Label>().Count().ShouldBe(0);
        rootGrid.Children.OfType<ContentView>().Select(Grid.GetRow).Distinct().Single().ShouldBe(0);
    }

    [Fact]
    public void StyleClasses_ShouldBeApplied_ToHeaderAndCellViews()
    {
        var titleView = new Label
        {
            StyleClass = new[] { "ExistingHeader" },
        };

        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            Columns =
            [
                new DataGridColumn
                {
                    Title = "Id",
                    TitleView = titleView,
                    HeaderStyleClass = "CustomHeader, ImportantHeader",
                    CellStyleClass = "CustomCell, ImportantCell",
                    ValueBinding = new Binding(nameof(DataGridRow.Id)),
                },
            ],
            ItemsSource = new List<DataGridRow>
            {
                new() { Id = 1, Name = "One" },
            },
        });

        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        var cell = rootGrid.Children.OfType<ContentView>().Single();

        titleView.StyleClass.ShouldContain("ExistingHeader");
        titleView.StyleClass.ShouldContain("CustomHeader");
        titleView.StyleClass.ShouldContain("ImportantHeader");
        cell.StyleClass.ShouldContain("CustomCell");
        cell.StyleClass.ShouldContain("ImportantCell");
    }

    [Fact]
    public void EmptyView_WithHeaders_ShouldUseStarRow()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            Columns =
            [
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
            ],
            ItemsSource = new List<DataGridRow>(),
        });

        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        rootGrid.RowDefinitions.Count.ShouldBe(3);
        rootGrid.RowDefinitions[0].Height.IsAuto.ShouldBeTrue();
        rootGrid.RowDefinitions[1].Height.IsAuto.ShouldBeTrue();
        rootGrid.RowDefinitions[2].Height.IsStar.ShouldBeTrue();
    }

    [Fact]
    public void SingleItem_WithoutHeaders_ShouldNotUseEmptyViewStarRow()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            ShowHeaders = false,
            Columns =
            [
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
            ],
            ItemsSource = new List<DataGridRow>
            {
                new() { Id = 1, Name = "One" },
            },
        });

        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        rootGrid.RowDefinitions.Any(x => x.Height.IsStar).ShouldBeFalse();
    }

    [Fact]
    public void DataGridValueBinding_GenericProvideValue_ShouldAttachBinding_WhenBindingContextBecomesBinding()
    {
        var target = AnimationReadyHandler.Prepare(new Entry());
        var source = new DataGridEditableRow { Name = "One" };
        var extension = new DataGridValueBindingExtension { Mode = BindingMode.TwoWay };

        var binding = ((IMarkupExtension<BindingBase>)extension)
            .ProvideValue(new DataGridValueBindingServiceProvider(target, Entry.TextProperty));

        binding.ShouldBeNull();

        target.BindingContext = new Binding(nameof(DataGridEditableRow.Name), source: source);

        target.Text.ShouldBe("One");

        target.Text = "Two";

        source.Name.ShouldBe("Two");
    }

    [Fact]
    public void UseAutoColumns_ShouldIgnoreProperties_WithDataGridIgnoreAttribute()
    {
        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            UseAutoColumns = true,
            ItemsSource = new List<AutoColumnRow>
            {
                new() { Id = 1, Name = "One", Hidden = "secret" },
            },
        });

        control.Columns.Select(column => column.Title).ShouldBe(["Identity", "Name"]);
        control.Columns.Select(column => ((Binding)column.ValueBinding).Path).ShouldBe([nameof(AutoColumnRow.Id), nameof(AutoColumnRow.Name)]);
    }

    [Fact]
    public void SelectionColumn_CheckBox_ShouldBeChecked_WhenSelectedItemsInitializedBeforeItemsSource()
    {
        var rows = new List<DataGridRow>
        {
            new() { Id = 1, Name = "One" },
            new() { Id = 2, Name = "Two" },
        };

        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            Columns =
            [
                new DataGridSelectionColumn(),
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
            ],
            SelectedItems = new ObservableCollection<object> { rows[1] },
            ItemsSource = rows,
        });

        var checkBox = GetSelectionCheckBox(control, rows[1]);

        checkBox.IsChecked.ShouldBeTrue();
    }

    [Fact]
    public void SelectionColumn_CheckBox_ShouldUpdate_WhenSelectedItemsCollectionChanges()
    {
        var rows = new List<DataGridRow>
        {
            new() { Id = 1, Name = "One" },
            new() { Id = 2, Name = "Two" },
        };
        var selectedItems = new ObservableCollection<object>();

        var control = AnimationReadyHandler.Prepare(new DataGrid
        {
            Columns =
            [
                new DataGridSelectionColumn(),
                new DataGridColumn { Title = "Id", ValueBinding = new Binding(nameof(DataGridRow.Id)) },
            ],
            ItemsSource = rows,
            SelectedItems = selectedItems,
        });

        var checkBox = GetSelectionCheckBox(control, rows[0]);
        checkBox.IsChecked.ShouldBeFalse();

        selectedItems.Add(rows[0]);
        checkBox.IsChecked.ShouldBeTrue();

        selectedItems.Remove(rows[0]);
        checkBox.IsChecked.ShouldBeFalse();
    }

    private static MaterialCheckBox GetSelectionCheckBox(DataGrid control, DataGridRow row)
    {
        var rootGrid = control.Content.ShouldBeOfType<Grid>();
        var selectionCell = rootGrid.Children.OfType<ContentView>()
            .Single(child => Grid.GetColumn(child) == 0 && child.BindingContext == row);

        return selectionCell.Content.ShouldBeOfType<ContentView>()
            .Content.ShouldBeOfType<MaterialCheckBox>();
    }

    private sealed class DataGridRow
    {
        public int Id { get; init; }

        public string Name { get; init; }
    }

    private sealed class DataGridEditableRow
    {
        public string Name { get; set; }
    }

    private sealed class DataGridValueBindingServiceProvider(View targetObject, BindableProperty targetProperty) : IServiceProvider, IProvideValueTarget
    {
        public object TargetObject { get; } = targetObject;

        public object TargetProperty { get; } = targetProperty;

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(IProvideValueTarget) ? this : null;
        }
    }

    private sealed class AutoColumnRow
    {
        [DisplayName("Identity")]
        public int Id { get; init; }

        public string Name { get; init; }

        [DataGridIgnore]
        public string Hidden { get; init; }
    }

    internal class DataGridTestViewModel : UraniumBindableObject
    {
        private Color lineSeparatorColor;

        public Color LineSeparatorColor { get => lineSeparatorColor; set => SetProperty(ref lineSeparatorColor, value); }
    }
}
