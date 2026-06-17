//using CommunityToolkit.Maui.Alerts;
//using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UraniumUI;
using UraniumUI.Resources;
using UraniumUI.Validations;

namespace UraniumApp.ViewModels;
public class ValidationsPageViewModel : UraniumBindableObject, IFormValidator
{
    private string email = string.Empty;
    private string fullName = string.Empty;
    private string asyncUserName = string.Empty;
    private string asyncValidationResult = string.Empty;
    private Gender? gender;
    private DateTime? birthDate;
    private TimeSpan? meetingTime;
    private int? numberOfSeats;
    private bool isTermsAndConditionsAccepted;

    public ValidationsPageViewModel()
    {
        SubmitCommand = new Command(() =>
        {
            //var snackbarOptions = new SnackbarOptions
            //{
            //    BackgroundColor = ColorResource.GetColor("Surface", "SurfaceDark"),
            //    TextColor = ColorResource.GetColor("OnSurface", "OnSurfaceDark"),
            //    ActionButtonTextColor = ColorResource.GetColor("Primary", "PrimaryDark"),
            //    CornerRadius = new CornerRadius(8),
            //};
            //Snackbar.Make($"Thank you {FullName}. You successfully registered!", duration: TimeSpan.FromSeconds(6), visualOptions: snackbarOptions)
            //.Show();
            ClearCommand.Execute(null);
        });

        ClearCommand = new Command(() =>
        {
            Email = string.Empty;
            FullName = string.Empty;
            Gender = default;
            BirthDate = default;
            MeetingTime = default;
            NumberOfSeats = default;
            IsTermsAndConditionsAccepted = default;
            AsyncUserName = string.Empty;
            AsyncValidationResult = string.Empty;
        });

        FillCommand = new Command(() =>
        {
            Email = "a@b.c";
            FullName = "Full Name";
            Gender = ViewModels.Gender.Male;
            BirthDate = DateTime.UtcNow.AddYears(-26);
            MeetingTime = new TimeSpan(10,00,00);
            NumberOfSeats = 2;
            IsTermsAndConditionsAccepted = true;
        });

        AsyncSubmitCommand = new Command(() =>
        {
            AsyncValidationResult = $"Username '{AsyncUserName}' is available.";
        });
    }

    public ICommand SubmitCommand { get; set; }

    public ICommand ClearCommand { get; set; }

    public ICommand FillCommand { get; set; }

    public ICommand AsyncSubmitCommand { get; set; }

    [EmailAddress]
    [Required]
    [MinLength(5)]
    public string Email { get => email; set => SetProperty(ref email, value); }

    [Required]
    [MinLength(3)]
    public string FullName { get => fullName; set => SetProperty(ref fullName, value); }
    public string AsyncUserName
    {
        get => asyncUserName;
        set
        {
            SetProperty(ref asyncUserName, value);
            AsyncValidationResult = string.Empty;
        }
    }

    public string AsyncValidationResult { get => asyncValidationResult; set => SetProperty(ref asyncValidationResult, value); }

    public Gender? Gender { get => gender; set => SetProperty(ref gender, value); }
    public DateTime? BirthDate { get => birthDate; set => SetProperty(ref birthDate, value); }
    public TimeSpan? MeetingTime { get => meetingTime; set => SetProperty(ref meetingTime, value); }
    public int? NumberOfSeats { get => numberOfSeats; set => SetProperty(ref numberOfSeats, value); }
    public bool IsTermsAndConditionsAccepted { get => isTermsAndConditionsAccepted; set => SetProperty(ref isTermsAndConditionsAccepted, value); }

    public async Task<FormValidationResult> ValidateAsync(FormValidationContext context)
    {
        AsyncValidationResult = string.Empty;
        await Task.Delay(2500, context.CancellationToken);

        var reservedNames = new[] { "admin", "root", "taken" };
        if (reservedNames.Contains(AsyncUserName?.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return FormValidationResult.PropertyError(
                nameof(AsyncUserName),
                "This username is already taken. Try 'uranium'.");
        }

        return FormValidationResult.Success();
    }
}
public enum Gender
{
    Other,
    Male,
    Female
}
