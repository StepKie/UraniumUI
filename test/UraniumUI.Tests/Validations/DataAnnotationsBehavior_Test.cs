using InputKit.Shared.Abstraction;
using InputKit.Shared.Validations;
using Microsoft.Maui.Controls.Internals;
using System.ComponentModel.DataAnnotations;
using UraniumUI.Tests.Core;
using UraniumUI.Validations;

namespace UraniumUI.Tests.Validations;

public class DataAnnotationsBehavior_Test
{
    public DataAnnotationsBehavior_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void Binding_ShouldAddValidation_FromDataAnnotations()
    {
        var view = new ValidatableView
        {
            BindingContext = new TestViewModel()
        };

        view.Behaviors.Add(new DataAnnotationsBehavior
        {
            Binding = new Binding(nameof(TestViewModel.Email))
        });

        var validation = Assert.Single(view.Validations);
        var dataAnnotationValidation = Assert.IsType<DataAnnotationValidation>(validation);
        Assert.IsType<RequiredAttribute>(dataAnnotationValidation.Attribute);
        Assert.Equal(nameof(TestViewModel.Email), dataAnnotationValidation.PropertyName);
    }

    [Fact]
    public void TypedBinding_ShouldAddValidation_FromDataAnnotations()
    {
        var view = new ValidatableView
        {
            BindingContext = new TestViewModel()
        };

        view.Behaviors.Add(new DataAnnotationsBehavior
        {
            Binding = new TypedBinding<TestViewModel, string>(
                source => (source.Email, true),
                (source, value) => source.Email = value,
                new Tuple<Func<TestViewModel, object>, string>[]
                {
                    new(static source => source, nameof(TestViewModel.Email))
                })
        });

        var validation = Assert.Single(view.Validations);
        var dataAnnotationValidation = Assert.IsType<DataAnnotationValidation>(validation);
        Assert.IsType<RequiredAttribute>(dataAnnotationValidation.Attribute);
        Assert.Equal(nameof(TestViewModel.Email), dataAnnotationValidation.PropertyName);
    }

    class ValidatableView : ContentView, IValidatable
    {
        public List<IValidation> Validations { get; } = new();

        public bool IsValid => Validations.All(validation => validation.Validate(null));

        public void DisplayValidation()
        {
        }

        public void ResetValidation()
        {
        }
    }

    class TestViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
