using PropertyChanged.SourceGenerator;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.EditorContext;

internal partial record TabButtonEntry(string Value)
{
    [Notify]
    private bool isActive = false;
    public SDUIEdges Margin => IsActive ? new(0, 2, -12, 2) : new(0, 2, 0, 2);
    public string Label = I18n.GetByKey($"gui.tab.{Value}");
}

internal partial record BaseWorkspaceContext(PackListingContext PackListing, ModConfigContext ModConfig)
{
    public readonly List<TabButtonEntry> AllTabs =
    [
        new("packs") { IsActive = true },
        new("textures"),
        new("config"),
        new("about"),
    ];

    [Notify]
    public string selectedTab = "packs";

    public void SelectTab(TabButtonEntry selected)
    {
        foreach (TabButtonEntry entry in AllTabs)
        {
            entry.IsActive = entry == selected;
        }
        SelectedTab = selected.Value;
    }
}
