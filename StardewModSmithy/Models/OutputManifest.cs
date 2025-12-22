using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models;

public sealed record OutputManifest(string Name, string Author)
{
    internal string PackFor { get; set; } = "???";
    internal string Desc { get; set; } = "???";
    internal string OutputFolder =>
        Path.Combine(ModEntry.DirectoryPath, ModEntry.EDITING_OUTPUT, Sanitize.Path(UniqueID));
    internal string TranslationFolder => Path.Combine(OutputFolder, "i18n");

    public string Version { get; set; } = "1.0.0";
    public string UniqueID = Sanitize.Key(string.Concat(Author, '.', Name));
    public string Description
    {
        get => string.IsNullOrEmpty(field) ? $"{Desc}, exported by {ModEntry.ModId}" : field;
        set => field = value;
    } = string.Empty;
    public object ContentPackFor => new { UniqueID = PackFor };
    public List<string> UpdateKeys = [];
}
