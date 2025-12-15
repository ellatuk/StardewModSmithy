using Newtonsoft.Json;
using StardewModdingAPI.Events;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;
using StardewValley;

namespace StardewModSmithy.Models;

public interface IMockPatch
{
    public string Action { get; }
    public Dictionary<string, object>? When { get; set; }
}

public sealed record MockEditData(string Target, Dictionary<string, object> Entries) : IMockPatch
{
    public string Action => "EditData";
    public Dictionary<string, object>? When { get; set; }
}

public record MockLoad(string Target, string FromFile) : IMockPatch
{
    public string Action => "Load";
    public Dictionary<string, object>? When { get; set; }

    public string Priority { get; set; } = AssetLoadPriority.Medium.ToString();
}

public sealed record MockInclude(string FromFile) : IMockPatch
{
    public string Action => "Include";
    public Dictionary<string, object>? When { get; set; }
}

internal record MockContent(List<IMockPatch> Changes);

internal sealed record MockContentMain(List<IMockPatch> Changes) : MockContent(Changes)
{
#pragma warning disable CA1822 // Mark members as static
    public string Format => ModEntry.ContentPatcherVersion;
#pragma warning restore CA1822 // Mark members as static
}

public sealed class OutputPackContentPatcher(OutputManifest manifest) : IOutputPack
{
    public const string PackFor = "Pathoschild.ContentPatcher";

    public List<ILoadableAsset> LoadableAssets = [];
    public List<IEditableAsset> EditableAssets = [];

    public static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    public void Save()
    {
        string targetPath = manifest.OutputFolder;
        Directory.CreateDirectory(targetPath);

        manifest.PackFor = PackFor;

        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
        string translationsDir = Path.Combine(targetPath, "i18n");

        if (Directory.Exists(dataDir))
            Directory.Delete(dataDir, true);
        Directory.CreateDirectory(dataDir);
        if (Directory.Exists(assetsDir))
            Directory.Delete(assetsDir, true);
        Directory.CreateDirectory(assetsDir);
        Directory.CreateDirectory(translationsDir);

        List<IMockPatch> changes = [];
        // translations
        Dictionary<string, string> translations = [];
        bool translationRequiresLoad = false;
        LocalizedContentManager.LanguageCode code = Game1.content.GetCurrentLanguage();
        foreach (IEditableAsset editable in EditableAssets)
        {
            translationRequiresLoad = translationRequiresLoad || editable.GetTranslations(ref translations);
        }
        if (translationRequiresLoad)
        {
            changes.Add(
                new MockLoad(TranslationString.I18N_Asset, "i18n/default.json")
                {
                    Priority = AssetLoadPriority.Low.ToString(),
                }
            );
            changes.Add(
                new MockLoad(TranslationString.I18N_Asset, $"i18n/{code}.json")
                {
                    When = new() { ["HasFile:{{FromFile}}"] = true },
                }
            );
        }
        // loads
        foreach (ILoadableAsset loadable in LoadableAssets)
        {
            changes.Add(new MockLoad(loadable.Target, loadable.FromFile));
            loadable.StageFiles(assetsDir);
        }
        // edits
        foreach (IEditableAsset editable in EditableAssets)
        {
            changes.Add(new MockInclude(Path.Combine("data", editable.IncludeName)));
            WriteJson(
                dataDir,
                editable.IncludeName,
                new MockContent([new MockEditData(editable.Target, editable.GetData())])
            );
        }

        // content.json
        WriteJson(targetPath, "content.json", new MockContentMain(changes));
        // manifest.json
        WriteJson(targetPath, "manifest.json", manifest);
        // i18n/{langaugecode}.json and i18n/default.json
        WriteJson(translationsDir, $"{code}.json", translations);
        WriteJson(translationsDir, "default.json", translations);
    }

    private static void WriteJson(string targetPath, string fileName, object content)
    {
        File.WriteAllText(
            Path.Combine(targetPath, fileName),
            JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings)
        );
    }

    public void Load()
    {
        string targetPath = manifest.OutputFolder;

        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
    }
}
