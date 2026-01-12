using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace StardewModSmithy.Wheels;

public enum AutosaveFrequencyMode
{
    Never = 0,
    OnExit = 1,
    OnSwitch = 2,
}

public sealed class ModConfig
{
    public string AuthorName
    {
        get => string.IsNullOrEmpty(field) ? "Smithy" : field;
        set => field = value;
    }
    public string AutosaveFrequency
    {
        get => AutosaveFrequencyEnumValue.ToString();
        set
        {
            if (Enum.TryParse(value, out AutosaveFrequencyMode parsed))
            {
                AutosaveFrequencyEnumValue = parsed;
            }
        }
    }
    public bool AutoSymlinkAndPatchReload { get; set; } = true;
    public KeybindList ShowWorkspaceKey { get; set; } = KeybindList.Parse("RightShift+F12");
    public KeybindList ToggleDragModeKey { get; set; } = new(SButton.MouseMiddle);
    public KeybindList SyncDragKey { get; set; } = new(SButton.MouseRight);

    internal AutosaveFrequencyMode AutosaveFrequencyEnumValue { get; set; } = AutosaveFrequencyMode.OnSwitch;
    internal Action<ModConfig>? doWriteConfig = null;

    internal void WriteConfig() => doWriteConfig?.Invoke(this);

    internal string GetAuthorName()
    {
        return string.IsNullOrEmpty(AuthorName) ? "Smithy" : AuthorName;
    }
}
