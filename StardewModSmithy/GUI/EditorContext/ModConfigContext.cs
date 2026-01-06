using System.ComponentModel;
using StardewModdingAPI.Utilities;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

internal sealed record ModConfigContext : INotifyPropertyChanged
{
    private readonly ModConfig config;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ModConfigContext(ModConfig Config)
    {
        config = Config;
        AutosaveFrequency.SelectedValue = config.AutosaveFrequency;
        AutosaveFrequency.ValueChanged += OnAutosaveFrequencyChange;
    }

    private void OnAutosaveFrequencyChange(object? sender, EventArgs e)
    {
        config.AutosaveFrequency = AutosaveFrequency.SelectedValue;
        config.WriteConfig();
    }

    public EnumSegmentsViewModel<AutosaveFrequencyMode> AutosaveFrequency = new();

    public string AuthorName
    {
        get => config.AuthorName;
        set
        {
            config.AuthorName = Sanitize.UniqueIDExclusionPattern.Replace(value, string.Empty);
            config.WriteConfig();
            PropertyChanged?.Invoke(this, new(nameof(AuthorName)));
        }
    }

    public bool AutoSymlinkAndPatchReload
    {
        get => config.AutoSymlinkAndPatchReload;
        set
        {
            config.AutoSymlinkAndPatchReload = value;
            config.WriteConfig();
            PropertyChanged?.Invoke(this, new(nameof(AutoSymlinkAndPatchReload)));
        }
    }

    public KeybindList ShowWorkspaceKey
    {
        get => config.ShowWorkspaceKey;
        set
        {
            config.ShowWorkspaceKey = value;
            config.WriteConfig();
            PropertyChanged?.Invoke(this, new(nameof(ShowWorkspaceKey)));
        }
    }
}
