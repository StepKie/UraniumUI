using Shouldly;
using UraniumUI.Extensions;
using UraniumUI.Material.Controls;
using UraniumUI.Tests.Core;
using UraniumUI.Views;

namespace UraniumUI.Material.Tests.Controls;

public class Chip_Test
{
    public Chip_Test()
    {
        ApplicationExtensions.CreateAndSetMockApplication();
    }

    [Fact]
    public void DestroyButton_ShouldExposeSemanticText()
    {
        var chip = new Chip { Text = "Ada" };
        var destroyButton = chip.FindManyInChildrenHierarchy<StatefulContentView>().Distinct().Single();

        SemanticProperties.GetDescription(destroyButton).ShouldBe("Remove Ada");
        SemanticProperties.GetHint(destroyButton).ShouldBe("Removes this selected item.");
    }
}
