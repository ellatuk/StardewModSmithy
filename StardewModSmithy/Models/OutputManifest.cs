using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.Models;

public sealed class OutputManifest()
{
    internal string PackFor { get; set; } = "???";
    internal string Desc { get; set; } = "???";
    internal string OutputFolder =>
        Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_OUTPUT, Sanitize.Path(UniqueID));
    internal string TranslationFolder => Path.Combine(OutputFolder, "i18n");
    internal HashSet<string> OptionalDependencies = [];

    public string Author { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string UniqueID { get; set; } = string.Empty;
    public string Description
    {
        get => string.IsNullOrEmpty(field) ? $"{Desc}, exported by {ModEntry.ModId}" : field;
        set => field = value;
    } = string.Empty;
    public object ContentPackFor => new { UniqueID = PackFor };
    public List<object>? Dependencies
    {
        get
        {
            List<object>? deps = null;
            if (OptionalDependencies.Any())
            {
                deps ??= [];
                deps.AddRange(OptionalDependencies.Select(dep => new { UniqueID = dep, IsRequired = false }));
            }
            return deps;
        }
    }
    public List<string> UpdateKeys = [];
    public string StardewModSmithy_ExportedAt => DateTime.Now.ToString(Game1.content.CurrentCulture);

    public static IEnumerable<OutputManifest> LoadAllFromOutputFolder()
    {
        foreach (string subdir in Directory.GetDirectories(Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_OUTPUT)))
        {
            string manifestPath = Path.Combine(subdir, Consts.MANIFEST_FILE);
            if (!File.Exists(manifestPath))
                continue;
            OutputManifest? manifest = ModEntry.ReadJson<OutputManifest>(manifestPath);
            if (manifest == null)
                continue;
            yield return manifest;
        }
    }
}
