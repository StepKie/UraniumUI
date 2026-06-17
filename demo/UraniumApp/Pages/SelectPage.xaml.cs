namespace UraniumApp.Pages;

public partial class SelectPage : ContentPage
{
    private DemoSelectItem selectedProfile;

    public IList<DemoSelectItem> Profiles { get; } = new List<DemoSelectItem>
    {
        new("Ada Lovelace", "First programmer", "A", Colors.MediumPurple),
        new("Grace Hopper", "Compiler pioneer", "G", Colors.DeepSkyBlue),
        new("Katherine Johnson", "Orbital mechanics", "K", Colors.SeaGreen),
        new("Margaret Hamilton", "Apollo flight software", "M", Colors.OrangeRed),
    };

    public DemoSelectItem SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (selectedProfile == value)
            {
                return;
            }

            selectedProfile = value;
            OnPropertyChanged(nameof(SelectedProfile));
        }
    }

    public SelectPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void SelectGrace_Clicked(object sender, EventArgs e)
    {
        SelectedProfile = Profiles[1];
    }

    private void Clear_Clicked(object sender, EventArgs e)
    {
        SelectedProfile = null;
    }

    public sealed record DemoSelectItem(string Name, string Description, string Initial, Color Color);
}
