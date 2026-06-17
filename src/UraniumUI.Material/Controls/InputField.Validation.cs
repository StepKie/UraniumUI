using InputKit.Shared.Abstraction;
using InputKit.Shared.Validations;
using UraniumUI.Pages;
using UraniumUI.Resources;
using UraniumUI.ViewExtensions;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace UraniumUI.Material.Controls;

public partial class InputField : IValidatable
{
    public List<IValidation> Validations { get; } = new();
    public bool IsValid { get => ValidationResults().All(x => x.isValid); }

    protected Lazy<ContentView> iconValidation = new Lazy<ContentView>(() => new ContentView
    {
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.End,
        Padding = new Thickness(BuiltInAttachmentLeftPadding, 0, 0, 0),
        Content = new Path
        {
            StyleClass = new[] { "InputField.ValidationIcon" },
            Fill = ColorResource.GetColor("Error", "ErrorDark", Colors.Red),
            Data = UraniumShapes.ExclamationCircle,
        }
    });

    protected Lazy<Label> labelValidation = new Lazy<Label>(() =>
    {
        var label = new Label
        {
            StyleClass = new[] { "InputField.ValidationLabel" },
            HorizontalOptions = LayoutOptions.Start,
            TextColor = ColorResource.GetColor("Error", "ErrorDark", Colors.Red),
        };

        label.SetId("ValidationLabel");

        return label;
    });

    protected bool lastValidationState = true;

    protected virtual void InitializeValidation()
    {
        // Keep it for the future requirements
    }

    protected virtual void CheckAndShowValidations()
    {
        var results = ValidationResults().ToArray();
        var isValidationPassed = results.All(a => a.isValid);

        var isStateChanged = isValidationPassed != lastValidationState;

        lastValidationState = isValidationPassed;

        if (isValidationPassed)
        {
            SetValidationSemanticMessage(null);

            if (isStateChanged)
            {
                endIconsContainer.Remove(iconValidation.Value);
                this.rootGrid.Remove(labelValidation.Value);
                OnPropertyChanged(nameof(IsValid));
			}
        }
        else
        {
            var message = string.Join('\n', results.Where(x => !x.isValid).Select(s => s.message));
            labelValidation.Value.Text = message;
            SemanticProperties.SetDescription(labelValidation.Value, message);
            SemanticProperties.SetDescription(iconValidation.Value, AccessibilityOptions.ValidationErrorDescription);
            SemanticProperties.SetHint(iconValidation.Value, message);
            SetValidationSemanticMessage(message);

            if (isStateChanged)
            {
                endIconsContainer.Add(iconValidation.Value);
                this.rootGrid.Add(labelValidation.Value, row: 1);
                OnPropertyChanged(nameof(IsValid));

                AnnounceValidationMessage(message);
			}
        }
    }

    protected IEnumerable<(bool isValid, string message)> ValidationResults()
    {
        foreach (var validation in Validations)
        {
            yield return ValidateOne(validation);
        }
    }

    protected (bool isValid, string message) ValidateOne(IValidation validation)
    {
        try
        {
            var value = GetValueForValidator();
            var validated = validation.Validate(value);
            return new(validated, validation.Message);
        }
        catch (Exception ex)
        {
            return new(false, ex.Message);
        }
    }

    protected virtual object GetValueForValidator()
    {
        return new object();
    }

    public virtual void DisplayValidation()
    {
        CheckAndShowValidations();
    }

    public virtual void ResetValidation()
    {
        endIconsContainer.Remove(iconValidation.Value);
        this.rootGrid.Remove(labelValidation.Value);
        SetValidationSemanticMessage(null);
        lastValidationState = true;
    }
}
