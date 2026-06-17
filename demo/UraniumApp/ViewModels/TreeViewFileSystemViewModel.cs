using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using UraniumUI;

namespace UraniumApp.ViewModels;
public class TreeViewFileSystemViewModel : UraniumBindableObject
{
    private static readonly EnumerationOptions FileSystemEnumerationOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
        ReturnSpecialDirectories = false,
        AttributesToSkip = 0,
    };

    public ObservableCollection<NodeItem> Nodes { get; private set; }

    public ReactiveCommand<NodeItem, Unit> LoadChildrenCommand { get; }

    public TreeViewFileSystemViewModel()
    {
        InitializeNodes();
        LoadChildrenCommand = ReactiveCommand.CreateFromTask<NodeItem>(LoadChildrenAsync);
    }

    void InitializeNodes()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            path = "C:\\";
        }

        Nodes = new ObservableCollection<NodeItem>(
            GetContent(path));
    }

    async Task LoadChildrenAsync(NodeItem node)
    {
        if (node is null || !node.IsDirectory || node.HasLoadedChildren)
        {
            return;
        }

        node.HasLoadedChildren = true;

        try
        {
            var children = await Task.Run(() => GetContent(node.Path).ToArray());

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                node.Children = new ObservableCollection<NodeItem>(children);
                node.IsLeaf = node.Children.Count == 0;
            });
        }
        catch (Exception ex)
        {
            node.HasLoadedChildren = false;
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }

    IEnumerable<NodeItem> GetContent(string dir)
    {
        foreach (var directory in Directory.EnumerateDirectories(dir, "*", FileSystemEnumerationOptions))
        {
            yield return new NodeItem
            {
                Name = Path.GetFileName(directory),
                Path = directory,
                IsDirectory = true,
                IsLeaf = false,
                IsExtended = false,
            };
        }

        foreach (var file in Directory.EnumerateFiles(dir, "*", FileSystemEnumerationOptions))
        {
            yield return new NodeItem
            {
                Name = Path.GetFileName(file),
                Path = file,
                IsDirectory = false,
                IsLeaf = true,
                IsExtended = true,
            };
        }
    }

    public class NodeItem : UraniumBindableObject
    {
        private bool isLeaf;
        private bool isExtended;
        private bool hasLoadedChildren;
        private IList<NodeItem> children = new ObservableCollection<NodeItem>();

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public virtual bool IsLeaf { get => isLeaf; set => SetProperty(ref isLeaf, value); }
        public virtual bool IsExtended { get => isExtended; set => SetProperty(ref isExtended, value); }
        public virtual bool HasLoadedChildren { get => hasLoadedChildren; set => SetProperty(ref hasLoadedChildren, value); }
        public virtual IList<NodeItem> Children { get => children; set => SetProperty(ref children, value ?? new ObservableCollection<NodeItem>()); }
    }
}
