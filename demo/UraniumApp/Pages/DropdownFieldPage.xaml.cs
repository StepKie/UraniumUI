namespace UraniumApp.Pages;

public partial class DropdownFieldPage : ContentPage
{
    public IList<DemoDropdownItem> TemplatedItems { get; } = new List<DemoDropdownItem>
    {
        new("Ada Lovelace", "First programmer", "A", Colors.MediumPurple),
        new("Grace Hopper", "Compiler pioneer", "G", Colors.DeepSkyBlue),
        new("Katherine Johnson", "Orbital mechanics", "K", Colors.SeaGreen),
        new("Margaret Hamilton", "Apollo flight software", "M", Colors.OrangeRed),
    };

    public DemoDropdownItem SelectedTemplatedItem { get; set; }

	public DropdownFieldPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        dropdown.SelectedItem = null;
    }

    public sealed record DemoDropdownItem(string Name, string Description, string Initial, Color Color);
}
