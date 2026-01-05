using System.ComponentModel;
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
            config.AuthorName = value;
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
}
