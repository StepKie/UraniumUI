#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;

namespace UraniumUI.Material.Controls;

public partial class TreeView
{
    private ListViewBase windowsCollectionView;
    private Panel windowsItemsPanel;
    private ScrollViewer windowsScrollViewer;
    private TransitionCollection windowsDefaultItemContainerTransitions;
    private TransitionCollection windowsDefaultChildrenTransitions;
    private double windowsScrollAnchorVerticalOffset = double.NaN;
    private int windowsScrollAnchorRestoreVersion;

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

    partial void CapturePlatformScrollAnchor()
    {
        windowsScrollViewer = FindWindowsScrollViewer();
        windowsScrollAnchorVerticalOffset = windowsScrollViewer?.VerticalOffset ?? double.NaN;
    }

    partial void RestorePlatformScrollAnchor(TreeViewNode anchorNode, int anchorIndex)
    {
        if (anchorNode is null || rootView?.Handler is null)
        {
            return;
        }

        var restoreVersion = ++windowsScrollAnchorRestoreVersion;
        void Restore()
        {
            if (restoreVersion != windowsScrollAnchorRestoreVersion || rootView?.Handler is null)
            {
                return;
            }

            var scrollViewer = windowsScrollViewer ?? FindWindowsScrollViewer();
            if (scrollViewer is not null && !double.IsNaN(windowsScrollAnchorVerticalOffset))
            {
                scrollViewer.ChangeView(null, windowsScrollAnchorVerticalOffset, null, disableAnimation: true);
                return;
            }

            var index = dataController.VisibleNodes.IndexOf(anchorNode);
            if (index < 0)
            {
                index = Math.Min(anchorIndex, dataController.VisibleNodes.Count - 1);
            }

            if (index >= 0)
            {
                rootView.ScrollTo(index, position: ScrollToPosition.Start, animate: false);
            }
        }

        if (rootView.Dispatcher?.DispatchDelayed(TimeSpan.FromMilliseconds(1), Restore) != true)
        {
            Restore();
        }
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
        windowsScrollViewer = null;
        windowsDefaultItemContainerTransitions = null;
        windowsDefaultChildrenTransitions = null;
    }

    private ScrollViewer FindWindowsScrollViewer()
    {
        return rootView?.Handler?.PlatformView is DependencyObject platformView
            ? FindDescendant<ScrollViewer>(platformView)
            : null;
    }

    private static T FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        if (root is null)
        {
            return null;
        }

        var childrenCount = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
#endif
