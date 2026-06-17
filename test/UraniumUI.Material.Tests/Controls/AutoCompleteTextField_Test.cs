using Shouldly;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;

namespace UraniumUI.Material.Tests.Controls;
public class AutoCompleteTextField_Test
{
    public AutoCompleteTextField_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void Text_BindingForInitialization_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());
        var viewModel = new { Text = "My Title" };
        control.BindingContext = viewModel;
        control.SetBinding(AutoCompleteTextField.TextProperty, new Binding(nameof(viewModel.Text)));

        // Assert
        control.Text.ShouldBe(viewModel.Text);
    }

    [Fact]
    public void Text_ShouldBeChanged_FromSource()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());
        var viewModel = new AutoCompleteTextFieldTestViewModel { Text = "My Title" };
        control.BindingContext = viewModel;
        control.SetBinding(AutoCompleteTextField.TextProperty, new Binding(nameof(viewModel.Text)));

        // Act
        viewModel.Text = "Title (changed)";

        // Assert
        control.Text.ShouldBe(viewModel.Text);
    }

    [Fact]
    public void Text_ShouldBeChanged_FromControl()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());
        var viewModel = new AutoCompleteTextFieldTestViewModel { Text = "My Title" };
        control.BindingContext = viewModel;
        control.SetBinding(AutoCompleteTextField.TextProperty, new Binding(nameof(viewModel.Text)));

        // Act
        control.Text = "Title (changed)";

        // Assert
        viewModel.Text.ShouldBe(control.Text);
    }

    [Fact]
    public void ClearIcon_HasAsymmetricLeftHitPadding()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());

        control.AllowClear = true;

        var clearIcon = control.Attachments.OfType<ContentView>().Single();

        clearIcon.Margin.ShouldBe(default(Thickness));
        clearIcon.Padding.ShouldBe(new Thickness(InputField.BuiltInAttachmentLeftPadding, 0, 0, 0));
    }

    [Fact]
    public void Keyboard_ShouldBeSet_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());
        var viewModel = new AutoCompleteTextFieldTestViewModel { Keyboard = Keyboard.Numeric };
        control.BindingContext = viewModel;

        // Act
        control.SetBinding(AutoCompleteTextField.KeyboardProperty, new Binding(nameof(viewModel.Keyboard)));

        // Assert
        control.Keyboard.ShouldBe(viewModel.Keyboard);
        control.AutoCompleteView.Keyboard.ShouldBe(viewModel.Keyboard);
    }

    [Fact]
    public void Keyboard_ShouldBeUpdated_FromViewModel()
    {
        var control = AnimationReadyHandler.Prepare(new AutoCompleteTextField());
        var viewModel = new AutoCompleteTextFieldTestViewModel { Keyboard = Keyboard.Numeric };
        control.BindingContext = viewModel;
        control.SetBinding(AutoCompleteTextField.KeyboardProperty, new Binding(nameof(viewModel.Keyboard)));

        // Act
        viewModel.Keyboard = Keyboard.Telephone;

        // Assert
        control.Keyboard.ShouldBe(viewModel.Keyboard);
        control.AutoCompleteView.Keyboard.ShouldBe(viewModel.Keyboard);
    }

    internal class AutoCompleteTextFieldTestViewModel : UraniumBindableObject
    {
        private string text;
        private Keyboard keyboard;

        public string Text { get => text; set => SetProperty(ref text, value); }

        public Keyboard Keyboard { get => keyboard; set => SetProperty(ref keyboard, value); }
    }
}
