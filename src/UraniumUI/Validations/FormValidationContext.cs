using UraniumUI.Controls;

namespace UraniumUI.Validations;

public sealed class FormValidationContext
{
    public FormValidationContext(FormView formView, object model, IServiceProvider services, CancellationToken cancellationToken)
    {
        FormView = formView;
        Model = model;
        Services = services;
        CancellationToken = cancellationToken;
    }

    public FormView FormView { get; }

    public object Model { get; }

    public IServiceProvider Services { get; }

    public CancellationToken CancellationToken { get; }

    public TModel GetModel<TModel>()
    {
        if (Model is TModel model)
        {
            return model;
        }

        throw new InvalidOperationException($"The validation model is not assignable to {typeof(TModel).FullName}.");
    }
}
