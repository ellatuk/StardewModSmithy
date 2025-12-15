using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models;

public sealed record OutputManifest(string Name, string Author)
{
    internal string PackFor { get; set; } = "???";
    internal string Desc { get; set; } = "Furniture pack";
    internal string OutputFolder =>
        Path.Combine(ModEntry.DirectoryPath, ModEntry.EDITING_OUTPUT, Sanitize.Path(UniqueID));

    public string Version { get; set; } = "1.0.0";
    public string UniqueID = Sanitize.Key(string.Concat(Author, '.', Name));
    public string Description => $"{Desc} exported by {ModEntry.ModId}";
    public object ContentPackFor => new { UniqueID = PackFor };
    public List<string> UpdateKeys = [];
}
