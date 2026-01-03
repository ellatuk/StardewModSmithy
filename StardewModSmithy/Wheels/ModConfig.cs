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

    internal Action<ModConfig>? doWriteConfig = null;

    internal void WriteConfig() => doWriteConfig?.Invoke(this);

    internal string GetAuthorName()
    {
        if (!string.IsNullOrEmpty(AuthorName))
            return AuthorName;
        if (Context.IsWorldReady)
            return Game1.player.displayName;
        return "Smithy";
    }
}
