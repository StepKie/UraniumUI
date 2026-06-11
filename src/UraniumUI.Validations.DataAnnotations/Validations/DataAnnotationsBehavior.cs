using InputKit.Shared.Abstraction;
using Microsoft.Maui.Controls.Internals;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace UraniumUI.Validations;

public class DataAnnotationsBehavior : Behavior<View>
{
    public BindingBase Binding { get; set; }

    protected BindableObject bindable;

    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);
        this.bindable = bindable;
        Apply();
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        Apply();

        bindable.BindingContextChanged -= Bindable_BindingContextChanged;
        bindable.BindingContextChanged += Bindable_BindingContextChanged;
    }

    protected override void OnDetachingFrom(View bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.BindingContextChanged -= Bindable_BindingContextChanged;
    }

    private void Bindable_BindingContextChanged(object sender, EventArgs e)
    {
        Apply();
    }

    void Apply()
    {
        if (bindable is not IValidatable validatable)
        {
            return;
        }

        var (source, path) = GetSourceAndPath();

        if (source is null || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var propertyInfo = GetProperty(source.GetType(), path);

        if (propertyInfo is null)
        {
            return;
        }

        var validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>(true);
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>(true);

        foreach (var attribute in validationAttributes)
        {
            validatable.Validations.Add(new DataAnnotationValidation(attribute, displayAttribute?.GetName() ?? propertyInfo.Name));
        }
    }

    (object Source, string Path) GetSourceAndPath()
    {
        return Binding switch
        {
            Binding binding => (binding.Source ?? bindable.BindingContext, binding.Path),
            TypedBindingBase typedBinding => (typedBinding.Source ?? bindable.BindingContext, GetTypedBindingPath(typedBinding)),
            _ => default
        };
    }

    static string GetTypedBindingPath(TypedBindingBase typedBinding)
    {
        var handlersField = typedBinding.GetType().GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic);

        if (handlersField?.GetValue(typedBinding) is not IEnumerable handlers)
        {
            return null;
        }

        var propertyNames = new List<string>();

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var propertyName = handler.GetType()
                .GetProperty("PropertyName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(handler) as string;

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            propertyNames.Add(propertyName);
        }

        return propertyNames.Count == 0 ? null : string.Join('.', propertyNames);
    }

    static PropertyInfo GetProperty(Type type, string propertyName)
    {
        var propertyNames = propertyName.Split('.');

        for (var i = 0; i < propertyNames.Length; i++)
        {
            var propertyInfo = type.GetProperty(propertyNames[i]);

            if (propertyInfo is null)
            {
                return null;
            }

            if (i == propertyNames.Length - 1)
            {
                return propertyInfo;
            }

            type = propertyInfo.PropertyType;
        }

        return null;
    }
}
