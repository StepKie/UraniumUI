using InputKit.Shared.Abstraction;
using InputKit.Shared.Validations;
using UraniumUI.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Validations;

namespace UraniumUI.Tests.Controls;

public class FormView_Test
{
    public FormView_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public async Task ValidateFormAsync_ShouldMapPropertyErrors_ToValidationPath()
    {
        var formView = new FormView
        {
            ValidationModel = new RegisterModel { UserName = "admin" },
            Validator = new TestFormValidator(_ => Task.FromResult(FormValidationResult.PropertyError(
                nameof(RegisterModel.UserName),
                "The username has already been taken.")))
        };

        var field = new ValidatableView();
        FormView.SetValidationPath(field, nameof(RegisterModel.UserName));
        formView.Children.Add(field);

        var result = await formView.ValidateFormAsync();

        Assert.False(result.IsValid);
        Assert.False(field.IsValid);
        Assert.True(field.DisplayValidationCalled);
        Assert.Single(field.Validations);
        Assert.Equal("The username has already been taken.", field.Validations[0].Message);
    }

    [Fact]
    public async Task SubmitAsync_ShouldToggleBusyIndicator_WhileValidatorRuns()
    {
        var validationCompletion = new TaskCompletionSource<FormValidationResult>();
        var submitted = false;
        var formView = new FormView
        {
            Validator = new TestFormValidator(_ => validationCompletion.Task),
            SubmitCommand = new Command(() => submitted = true)
        };

        var busyIndicator = new ActivityIndicator();
        FormView.SetIsBusyIndicator(busyIndicator, true);
        formView.Children.Add(busyIndicator);

        var submitTask = formView.SubmitAsync();

        Assert.True(formView.IsBusy);
        Assert.True(formView.IsValidating);
        Assert.True(busyIndicator.IsVisible);
        Assert.True(busyIndicator.IsRunning);

        validationCompletion.SetResult(FormValidationResult.Success());

        Assert.True(await submitTask);
        Assert.True(submitted);
        Assert.False(formView.IsBusy);
        Assert.False(formView.IsValidating);
        Assert.False(busyIndicator.IsVisible);
        Assert.False(busyIndicator.IsRunning);
    }

    [Fact]
    public async Task SubmitAsync_ShouldNotRunAsyncValidator_WhenLocalValidationFails()
    {
        var validatorCalled = false;
        var formView = new FormView
        {
            Validator = new TestFormValidator(_ =>
            {
                validatorCalled = true;
                return Task.FromResult(FormValidationResult.Success());
            })
        };

        var field = new ValidatableView();
        field.Validations.Add(new FailingValidation("Local validation failed."));
        formView.Children.Add(field);

        var result = await formView.SubmitAsync();

        Assert.False(result);
        Assert.False(validatorCalled);
        Assert.True(field.DisplayValidationCalled);
    }

    [Fact]
    public async Task InheritedSubmitCommand_ShouldUseAsyncSubmitOverride()
    {
        var validatorCalled = new TaskCompletionSource();
        var formView = new CommandProbeFormView
        {
            Validator = new TestFormValidator(_ =>
            {
                validatorCalled.SetResult();
                return Task.FromResult(FormValidationResult.Success());
            })
        };

        formView.ExecuteInheritedSubmitCommand();

        await validatorCalled.Task.WaitAsync(TimeSpan.FromSeconds(1));
    }

    private sealed class TestFormValidator : IFormValidator
    {
        private readonly Func<FormValidationContext, Task<FormValidationResult>> validate;

        public TestFormValidator(Func<FormValidationContext, Task<FormValidationResult>> validate)
        {
            this.validate = validate;
        }

        public Task<FormValidationResult> ValidateAsync(FormValidationContext context)
        {
            return validate(context);
        }
    }

    private sealed class ValidatableView : ContentView, IValidatable
    {
        public List<IValidation> Validations { get; } = new();

        public bool IsValid => Validations.All(validation => validation.Validate(null));

        public bool DisplayValidationCalled { get; private set; }

        public void DisplayValidation()
        {
            DisplayValidationCalled = true;
        }

        public void ResetValidation()
        {
            DisplayValidationCalled = false;
        }
    }

    private sealed class CommandProbeFormView : FormView
    {
        public void ExecuteInheritedSubmitCommand()
        {
            buttonSubmitCommand.Execute(null);
        }
    }

    private sealed class FailingValidation : IValidation
    {
        public FailingValidation(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public bool Validate(object value) => false;
    }

    private sealed class RegisterModel
    {
        public string UserName { get; set; }
    }
}
