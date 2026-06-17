using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;
using UraniumUI.Extensions;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.Views;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

public class TextFieldPasswordShowHideAttachment : StatefulContentView
{
    public TextField TextField { get; protected set; }

    public TextFieldPasswordShowHideAttachment()
    {
        VerticalOptions = LayoutOptions.Center;
        Padding = new Thickness(InputField.BuiltInAttachmentLeftPadding, 0, 0, 0);
        TappedCommand = new Command(SwitchPassword);
    }

    protected override void OnParentSet()
    {
        if (TextField is not null)
        {
            TextField.PropertyChanged -= TextField_PropertyChanged;
        }

        TextField = this.FindInParents<TextField>();
        if (TextField == null)
        {
            UpdateSemanticProperties();
            return;
        }

        TextField.PropertyChanged += TextField_PropertyChanged;
        UpdateIcon();
    }

    private void TextField_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TextField.IsPassword))
        {
            UpdateIcon();
        }
    }

    protected virtual void SwitchPassword(object parameter)
    {
        if (TextField is null)
        {
            UpdateIcon();
            return;
        }

        TextField.IsPassword = !TextField.IsPassword;
        UpdateIcon();
    }

    protected void UpdateIcon()
    {
        if (TextField is null)
        {
            Content = null;

            return;
        }

        Content = TextField.IsPassword ? GetPathFromData(UraniumShapes.Eye) : GetPathFromData(UraniumShapes.EyeSlash);
        UpdateSemanticProperties();
    }

    private void UpdateSemanticProperties()
    {
        var options = AccessibilityOptionsProvider.Get();
        var description = TextField?.IsPassword == true ? options.ShowPasswordDescription : options.HidePasswordDescription;

        SemanticProperties.SetDescription(this, description);
        SemanticProperties.SetHint(this, options.PasswordVisibilityToggleHint);
    }

    private Path GetPathFromData(Geometry data)
    {
        return new Path
        {
            Fill = ColorResource.GetColor("OnBackground", "OnBackgroundDark", Colors.DarkGray).WithAlpha(.5f),
            VerticalOptions = LayoutOptions.Center,
            Data = data,
        };
    }
}
