using Newtonsoft.Json;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models;

public sealed class OutputManifest()
{
    internal string PackFor { get; set; } = "???";
    internal string Desc { get; set; } = "???";
    internal string OutputFolder =>
        Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_OUTPUT, Sanitize.Path(UniqueID));
    internal string TranslationFolder => Path.Combine(OutputFolder, "i18n");

    public string Author { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string UniqueID => string.Concat(Sanitize.UniqueID(Author), '.', Sanitize.UniqueID(Name));
    public string Description
    {
        get => string.IsNullOrEmpty(field) ? $"{Desc}, exported by {ModEntry.ModId}" : field;
        set => field = value;
    } = string.Empty;
    public object ContentPackFor => new { UniqueID = PackFor };
    public List<string> UpdateKeys = [];

    public static IEnumerable<OutputManifest> LoadAllFromOutputFolder()
    {
        foreach (string subdir in Directory.GetDirectories(Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_OUTPUT)))
        {
            string manifestPath = Path.Combine(subdir, Consts.MANIFEST_FILE);
            if (!File.Exists(manifestPath))
                continue;
            OutputManifest? manifest = JsonConvert.DeserializeObject<OutputManifest>(File.ReadAllText(manifestPath));
            if (manifest == null)
                continue;
            yield return manifest;
        }
    }
}
