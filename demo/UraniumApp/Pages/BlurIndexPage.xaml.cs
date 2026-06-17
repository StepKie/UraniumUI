using System.ComponentModel;
using UraniumApp.Pages.Blurs;
using UraniumUI.Blurs;

namespace UraniumApp.Pages;

public partial class BlurIndexPage : ContentPage
{
	public BlurIndexPage()
	{
		InitializeComponent();

        BackgroundImage.Loaded += BackgroundImage_Loaded;
        BackgroundImage.PropertyChanged += BackgroundImage_PropertyChanged;
	}

    private void BackgroundImage_Loaded(object sender, EventArgs e)
    {
        if (!BackgroundImage.IsLoading)
        {
            InvalidateBlurAfterImageLoadingFinished();
        }
    }

    private void BackgroundImage_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Image.IsLoading) && !BackgroundImage.IsLoading)
        {
            InvalidateBlurAfterImageLoadingFinished();
        }
    }

    private void InvalidateBlurAfterImageLoadingFinished()
    {
        Dispatcher.Dispatch(InvalidateBlur);
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), InvalidateBlur);
    }

    private void InvalidateBlur()
    {
        foreach (var blurEffect in BlurHost.Effects.OfType<BlurEffect>())
        {
            blurEffect.InvalidateAndroidBlur();
        }
    }

    private void GoToPreviewPage(object sender, EventArgs e)
    {
		this.Navigation.PushAsync(new BlursPreviewPage());
    }
}
