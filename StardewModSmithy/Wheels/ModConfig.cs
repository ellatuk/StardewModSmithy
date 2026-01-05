using StardewModdingAPI;
using StardewValley;

namespace StardewModSmithy.Wheels;

public enum AutosaveFrequencyMode
{
    Never = 0,
    OnExit = 1,
    OnAdd = 2,
}

public sealed class ModConfig
{
    public string AuthorName { get; set; } = "";
    public AutosaveFrequencyMode AutosaveFrequency { get; set; } = AutosaveFrequencyMode.OnAdd;
    public bool AutoSymlinkAndPatchReload { get; set; } = true;

    internal Action<ModConfig>? doWriteConfig = null;

    internal void WriteConfig() => doWriteConfig?.Invoke(this);

    internal string GetAuthorName()
    {
        return string.IsNullOrEmpty(AuthorName) ? "Smithy" : AuthorName;
    }
}
