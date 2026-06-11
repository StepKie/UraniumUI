using InputKit.Shared.Abstraction;
using UraniumUI.Validations;
using InputKitFormView = InputKit.Shared.Controls.FormView;

namespace UraniumUI.Controls;

public class FormView : InputKitFormView
{
    private readonly Label validationSummaryLabel;
    private readonly Dictionary<View, bool> busyDisabledViews = new();

    public FormView()
    {
        validationSummaryLabel = CreateValidationSummaryLabel();
        Children.Insert(0, validationSummaryLabel);
        ChildAdded += OnChildAdded;
        ChildRemoved += OnChildRemoved;
    }

    public IFormValidator Validator { get => (IFormValidator)GetValue(ValidatorProperty); set => SetValue(ValidatorProperty, value); }

    public static readonly BindableProperty ValidatorProperty = BindableProperty.Create(
        nameof(Validator),
        typeof(IFormValidator),
        typeof(FormView));

    public FormValidationHandler ValidationHandler { get => (FormValidationHandler)GetValue(ValidationHandlerProperty); set => SetValue(ValidationHandlerProperty, value); }

    public static readonly BindableProperty ValidationHandlerProperty = BindableProperty.Create(
        nameof(ValidationHandler),
        typeof(FormValidationHandler),
        typeof(FormView));

    public object ValidationModel { get => GetValue(ValidationModelProperty); set => SetValue(ValidationModelProperty, value); }

    public static readonly BindableProperty ValidationModelProperty = BindableProperty.Create(
        nameof(ValidationModel),
        typeof(object),
        typeof(FormView));

    public bool ShowValidationSummary { get => (bool)GetValue(ShowValidationSummaryProperty); set => SetValue(ShowValidationSummaryProperty, value); }

    public static readonly BindableProperty ShowValidationSummaryProperty = BindableProperty.Create(
        nameof(ShowValidationSummary),
        typeof(bool),
        typeof(FormView),
        true,
        propertyChanged: (bindable, oldValue, newValue) => ((FormView)bindable).ApplyValidationSummaryVisibility());

    private static readonly BindablePropertyKey IsBusyPropertyKey = BindableProperty.CreateReadOnly(
        nameof(IsBusy),
        typeof(bool),
        typeof(FormView),
        false,
        propertyChanged: (bindable, oldValue, newValue) => ((FormView)bindable).ApplyBusyState());

    public static readonly BindableProperty IsBusyProperty = IsBusyPropertyKey.BindableProperty;

    public bool IsBusy => (bool)GetValue(IsBusyProperty);

    private static readonly BindablePropertyKey IsValidatingPropertyKey = BindableProperty.CreateReadOnly(
        nameof(IsValidating),
        typeof(bool),
        typeof(FormView),
        false);

    public static readonly BindableProperty IsValidatingProperty = IsValidatingPropertyKey.BindableProperty;

    public bool IsValidating => (bool)GetValue(IsValidatingProperty);

    private static readonly BindablePropertyKey FormValidationResultPropertyKey = BindableProperty.CreateReadOnly(
        nameof(FormValidationResult),
        typeof(FormValidationResult),
        typeof(FormView),
        null);

    public static readonly BindableProperty FormValidationResultProperty = FormValidationResultPropertyKey.BindableProperty;

    public FormValidationResult FormValidationResult => (FormValidationResult)GetValue(FormValidationResultProperty);

    public new static readonly BindableProperty IsSubmitButtonProperty = BindableProperty.CreateAttached(
        "IsSubmitButton",
        typeof(bool),
        typeof(FormView),
        false,
        propertyChanged: (bindable, oldValue, newValue) => InputKitFormView.SetIsSubmitButton(bindable, (bool)newValue));

    public new static bool GetIsSubmitButton(BindableObject view) => (bool)view.GetValue(IsSubmitButtonProperty);

    public new static void SetIsSubmitButton(BindableObject view, bool value) => view.SetValue(IsSubmitButtonProperty, value);

    public new static readonly BindableProperty IsResetButtonProperty = BindableProperty.CreateAttached(
        "IsResetButton",
        typeof(bool),
        typeof(FormView),
        false,
        propertyChanged: (bindable, oldValue, newValue) => InputKitFormView.SetIsResetButton(bindable, (bool)newValue));

    public new static bool GetIsResetButton(BindableObject view) => (bool)view.GetValue(IsResetButtonProperty);

    public new static void SetIsResetButton(BindableObject view, bool value) => view.SetValue(IsResetButtonProperty, value);

    public static readonly BindableProperty ValidationPathProperty = BindableProperty.CreateAttached(
        "ValidationPath",
        typeof(string),
        typeof(FormView),
        null);

    public static string GetValidationPath(BindableObject view) => (string)view.GetValue(ValidationPathProperty);

    public static void SetValidationPath(BindableObject view, string value) => view.SetValue(ValidationPathProperty, value);

    public static readonly BindableProperty IsBusyIndicatorProperty = BindableProperty.CreateAttached(
        "IsBusyIndicator",
        typeof(bool),
        typeof(FormView),
        false,
        propertyChanged: OnIsBusyIndicatorChanged);

    public static bool GetIsBusyIndicator(BindableObject view) => (bool)view.GetValue(IsBusyIndicatorProperty);

    public static void SetIsBusyIndicator(BindableObject view, bool value) => view.SetValue(IsBusyIndicatorProperty, value);

    public override async void Submit()
    {
        await SubmitAsync();
    }

    public virtual async Task<bool> SubmitAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return false;
        }

        var result = await ValidateFormAsync(cancellationToken);
        if (!result.IsValid)
        {
            return false;
        }

        SetIsValidated(true);

        if (SubmitCommand?.CanExecute(true) ?? true)
        {
            SubmitCommand?.Execute(true);
        }

        return true;
    }

    public virtual async Task<FormValidationResult> ValidateFormAsync(CancellationToken cancellationToken = default)
    {
        ClearFormValidationMessages();

        if (!InputKitFormView.CheckValidation(this))
        {
            DisplayChildValidations();
            var invalidResult = FormValidationResult.Invalid();
            SetFormValidationResult(invalidResult);
            SetIsValidated(false);
            return invalidResult;
        }

        if (Validator is null && ValidationHandler is null)
        {
            var successResult = FormValidationResult.Success();
            SetFormValidationResult(successResult);
            SetIsValidated(true);
            return successResult;
        }

        SetBusy(true);

        try
        {
            var result = await RunFormValidatorAsync(cancellationToken) ?? FormValidationResult.Success();
            ApplyFormValidationResult(result);
            SetIsValidated(result.IsValid);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var result = FormValidationResult.Invalid();
            SetFormValidationResult(result);
            SetIsValidated(false);
            return result;
        }
        catch (Exception ex)
        {
            var result = FormValidationResult.Error(ex.Message);
            ApplyFormValidationResult(result);
            SetIsValidated(false);
            return result;
        }
        finally
        {
            SetBusy(false);
        }
    }

    public override void Reset()
    {
        ClearFormValidationMessages();
        base.Reset();
    }

    protected virtual FormValidationContext CreateValidationContext(CancellationToken cancellationToken)
    {
        return new FormValidationContext(this, ValidationModel ?? BindingContext, GetServiceProvider(), cancellationToken);
    }

    protected virtual IServiceProvider GetServiceProvider()
    {
        if (Handler?.MauiContext?.Services is { } services)
        {
            return services;
        }

        try
        {
            return UraniumServiceProvider.Current;
        }
        catch
        {
            return null;
        }
    }

    protected virtual Label CreateValidationSummaryLabel()
    {
        return new Label
        {
            IsVisible = false,
            TextColor = Colors.Red,
            StyleClass = new[] { "FormView.ValidationSummary" },
        };
    }

    private Task<FormValidationResult> RunFormValidatorAsync(CancellationToken cancellationToken)
    {
        var context = CreateValidationContext(cancellationToken);

        if (Validator is not null)
        {
            return Validator.ValidateAsync(context);
        }

        return ValidationHandler(context);
    }

    private void ApplyFormValidationResult(FormValidationResult result)
    {
        SetFormValidationResult(result);

        if (result.IsValid)
        {
            HideValidationSummary();
            return;
        }

        var summaryMessages = new List<string>();

        foreach (var error in result.Errors)
        {
            if (error.IsFormLevel)
            {
                summaryMessages.Add(error.Message);
                continue;
            }

            var matched = false;

            foreach (var memberName in error.MemberNames)
            {
                foreach (var (validatable, _) in GetValidatablesByPath(memberName))
                {
                    validatable.Validations.Add(new FormValidationMessage(error.Message));
                    validatable.DisplayValidation();
                    matched = true;
                }
            }

            if (!matched)
            {
                summaryMessages.Add(error.Message);
            }
        }

        ShowValidationSummaryMessages(summaryMessages);
    }

    private void ClearFormValidationMessages()
    {
        foreach (var (validatable, _) in GetChildValidatables())
        {
            validatable.Validations.RemoveAll(x => x is FormValidationMessage);
        }

        HideValidationSummary();
        SetFormValidationResult(null);
    }

    private void DisplayChildValidations()
    {
        foreach (var (validatable, _) in GetChildValidatables())
        {
            validatable.DisplayValidation();
        }
    }

    private IEnumerable<(IValidatable validatable, BindableObject bindable)> GetValidatablesByPath(string validationPath)
    {
        foreach (var (validatable, bindable) in GetChildValidatables())
        {
            if (string.Equals(GetValidationPath(bindable), validationPath, StringComparison.Ordinal))
            {
                yield return (validatable, bindable);
            }
        }
    }

    private IEnumerable<(IValidatable validatable, BindableObject bindable)> GetChildValidatables()
    {
        foreach (var bindable in GetChildBindableObjects(this))
        {
            if (bindable is IValidatable validatable)
            {
                yield return (validatable, bindable);
            }
        }
    }

    private static IEnumerable<BindableObject> GetChildBindableObjects(Element parent)
    {
        foreach (var child in GetDirectChildElements(parent))
        {
            if (child is BindableObject bindableObject)
            {
                yield return bindableObject;
            }

            foreach (var descendant in GetChildBindableObjects(child))
            {
                yield return descendant;
            }
        }
    }

    private static IEnumerable<Element> GetDirectChildElements(Element parent)
    {
        if (parent is Layout layout)
        {
            foreach (var child in layout.Children.OfType<Element>())
            {
                yield return child;
            }
        }

        if (parent is ContentView contentView && contentView.Content is Element content)
        {
            yield return content;
        }

        if (parent is ScrollView scrollView && scrollView.Content is Element scrollContent)
        {
            yield return scrollContent;
        }

        if (parent is Border border && border.Content is Element borderContent)
        {
            yield return borderContent;
        }
    }

    private void ShowValidationSummaryMessages(IEnumerable<string> messages)
    {
        var distinctMessages = messages.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
        validationSummaryLabel.Text = string.Join(Environment.NewLine, distinctMessages);
        ApplyValidationSummaryVisibility();
    }

    private void HideValidationSummary()
    {
        validationSummaryLabel.Text = string.Empty;
        validationSummaryLabel.IsVisible = false;
    }

    private void ApplyValidationSummaryVisibility()
    {
        validationSummaryLabel.IsVisible = ShowValidationSummary && !string.IsNullOrWhiteSpace(validationSummaryLabel.Text);
    }

    private void SetBusy(bool isBusy)
    {
        SetValue(IsValidatingPropertyKey, isBusy);
        SetValue(IsBusyPropertyKey, isBusy);
    }

    private void SetFormValidationResult(FormValidationResult result)
    {
        SetValue(FormValidationResultPropertyKey, result);
    }

    private void SetIsValidated(bool isValidated)
    {
        SetValue(InputKitFormView.IsValidatedProperty, isValidated);
    }

    private void ApplyBusyState()
    {
        if (!IsBusy)
        {
            RestoreBusyDisabledViews();
        }

        foreach (var bindable in GetChildBindableObjects(this))
        {
            if (bindable is not View view)
            {
                continue;
            }

            if (GetIsBusyIndicator(view))
            {
                ApplyBusyIndicatorState(view, IsBusy);
            }

            if (IsBusy && (IsSubmitElement(view) || IsResetElement(view)))
            {
                ApplyBusyEnabledState(view);
            }
        }
    }

    private void RestoreBusyDisabledViews()
    {
        foreach (var (view, wasEnabled) in busyDisabledViews)
        {
            view.IsEnabled = wasEnabled;
        }

        busyDisabledViews.Clear();
    }

    private void ApplyBusyEnabledState(View view)
    {
        if (IsBusy)
        {
            if (!busyDisabledViews.ContainsKey(view))
            {
                busyDisabledViews[view] = view.IsEnabled;
            }

            view.IsEnabled = false;
            return;
        }

        RestoreBusyDisabledViews();
    }

    private static void ApplyBusyIndicatorState(View view, bool isBusy)
    {
        view.IsVisible = isBusy;

        if (view is ActivityIndicator activityIndicator)
        {
            activityIndicator.IsRunning = isBusy;
        }
    }

    private static bool IsSubmitElement(BindableObject view)
    {
        return GetIsSubmitButton(view) || InputKitFormView.GetIsSubmitButton(view);
    }

    private static bool IsResetElement(BindableObject view)
    {
        return GetIsResetButton(view) || InputKitFormView.GetIsResetButton(view);
    }

    private static void OnIsBusyIndicatorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View view)
        {
            return;
        }

        if ((bool)newValue)
        {
            ApplyBusyIndicatorState(view, FindParentFormView(view)?.IsBusy ?? false);
        }
    }

    private static FormView FindParentFormView(Element element)
    {
        var parent = element.Parent;

        while (parent is not null)
        {
            if (parent is FormView formView)
            {
                return formView;
            }

            parent = parent.Parent;
        }

        return null;
    }

    private void OnChildAdded(object sender, ElementEventArgs e)
    {
        ApplyBusyState();
    }

    private void OnChildRemoved(object sender, ElementEventArgs e)
    {
        if (!IsBusy && e.Element is View view)
        {
            busyDisabledViews.Remove(view);
        }
    }
}
