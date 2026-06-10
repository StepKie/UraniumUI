#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace UraniumUI.Material.Controls;

public partial class TreeView
{
    private ListViewBase windowsCollectionView;
    private Panel windowsItemsPanel;
    private TransitionCollection windowsDefaultItemContainerTransitions;
    private TransitionCollection windowsDefaultChildrenTransitions;

    partial void UpdatePlatformAnimationState()
    {
        if (rootView?.Handler?.PlatformView is not ListViewBase listView)
        {
            DetachWindowsCollectionView();
            return;
        }

        if (!ReferenceEquals(windowsCollectionView, listView))
        {
            DetachWindowsCollectionView();
            windowsCollectionView = listView;
            windowsDefaultItemContainerTransitions = listView.ItemContainerTransitions;
            listView.Loaded += WindowsCollectionView_Loaded;
        }

        listView.ItemContainerTransitions = UseAnimation
            ? windowsDefaultItemContainerTransitions
            : new TransitionCollection();

        UpdateWindowsItemsPanelAnimationState(listView.ItemsPanelRoot as Panel);
    }

    private void WindowsCollectionView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdatePlatformAnimationState();
    }

    private void UpdateWindowsItemsPanelAnimationState(Panel panel)
    {
        if (panel is null)
        {
            return;
        }

        if (!ReferenceEquals(windowsItemsPanel, panel))
        {
            windowsItemsPanel = panel;
            windowsDefaultChildrenTransitions = panel.ChildrenTransitions;
        }

        panel.ChildrenTransitions = UseAnimation
            ? windowsDefaultChildrenTransitions
            : new TransitionCollection();
    }

    private void DetachWindowsCollectionView()
    {
        if (windowsCollectionView is not null)
        {
            windowsCollectionView.Loaded -= WindowsCollectionView_Loaded;
        }

        windowsCollectionView = null;
        windowsItemsPanel = null;
        windowsDefaultItemContainerTransitions = null;
        windowsDefaultChildrenTransitions = null;
    }
}
#endif
