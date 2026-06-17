namespace UraniumUI.Dialogs;
public class DefaultDialogAnimatedContentPage : ContentPage
{
	private Task closeTask;

	public DefaultDialogAnimatedContentPage()
	{
        Loaded += OnLoaded;
	}

    private void OnLoaded(object sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        if (Content is null)
        {
            return;
        }

        Content.Opacity = 0;
        Content.Scale = 0.8;

        Content.FadeTo(1, 250, Easing.CubicInOut);
        Content.ScaleTo(1, 250, Easing.CubicInOut);
    }

    public Task CloseAsync()
    {
        return closeTask ??= CloseCoreAsync();
    }

    private async Task CloseCoreAsync()
    {
        if (Content is not null)
        {
            var tasks = new Task[]
            {
                Content.FadeTo(0, 250, Easing.CubicInOut),
                Content.ScaleTo(0.8, 250, Easing.CubicInOut)
            };

            await Task.WhenAll(tasks);
        }

        if (Navigation.ModalStack.LastOrDefault() == this)
        {
            await Navigation.PopModalAsync(animated: false);
        }
    }
}
