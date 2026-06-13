using Shouldly;
using UraniumUI.Tests.Core;
using UraniumUI.Theming;

namespace UraniumUI.Tests.Theming;

public class DynamicTint_Test
{
    public DynamicTint_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void TransparentBackground_ShouldStayTransparent_WhenTintChanges()
    {
        var view = new Grid
        {
            BackgroundColor = Colors.Transparent
        };

        DynamicTint.SetBackgroundColorOpacity(view, 0.8f);

        view.BackgroundColor.ShouldBe(Colors.Transparent);
    }

    [Fact]
    public void BackgroundColorChange_ShouldRespectCurrentTintOpacity()
    {
        var view = new Grid
        {
            BackgroundColor = Colors.Red
        };

        DynamicTint.SetBackgroundColorOpacity(view, 0.8f);
        view.BackgroundColor.ShouldBe(Colors.Red.WithAlpha(0.8f));

        view.BackgroundColor = Colors.Blue;

        view.BackgroundColor.ShouldBe(Colors.Blue.WithAlpha(0.8f));
    }
}
