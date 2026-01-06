using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;
using StardewModSmithy.Wheels;
using StardewValley.Extensions;

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
    public string[]? TargetField { get; set; }
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

internal record MockContent(List<IMockPatch> Changes)
{
    [JsonProperty("$schema")]
    public string JsonSchema => "https://smapi.io/schemas/content-patcher.json";
}

internal record MockContentEditDataInclude(List<MockEditData> Changes);

internal sealed record MockContentMain(List<IMockPatch> Changes) : MockContent(Changes)
{
#pragma warning disable CA1822 // Mark members as static
    public string Format => ModEntry.ContentPatcherVersion;
#pragma warning restore CA1822 // Mark members as static
}

public sealed class OutputPackContentPatcher(OutputManifest manifest) : IOutputPack
{
    public const string PackFor = "Pathoschild.ContentPatcher";
    public OutputManifest Manifest => manifest;

    public TextureAssetGroup? TextureAssetGroup { get; set; } = null;
    public FurnitureAsset? FurniAsset { get; set; } = null;
    public WallpaperFlooringAsset? WallAndFloorAsset { get; set; } = null;

    public IEnumerable<ILoadableAsset> LoadableAssets
    {
        get
        {
            if (TextureAssetGroup is not null)
                yield return TextureAssetGroup;
        }
    }
    public IEnumerable<IEditableAsset> EditableAssets
    {
        get
        {
            if (FurniAsset is not null)
                yield return FurniAsset;
            if (WallAndFloorAsset is not null)
                yield return WallAndFloorAsset;
        }
    }

    public TranslationStore? Translations = TranslationStore.FromSourceDir(manifest.TranslationFolder);

    public void Save()
    {
        string targetPath = manifest.OutputFolder;
        Directory.CreateDirectory(targetPath);

        manifest.PackFor = PackFor;

        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
        string translationsDir = manifest.TranslationFolder;

        if (Directory.Exists(dataDir))
            Directory.Delete(dataDir, true);
        Directory.CreateDirectory(dataDir);
        if (Directory.Exists(assetsDir))
            Directory.Delete(assetsDir, true);
        Directory.CreateDirectory(assetsDir);

        List<IMockPatch> changes = [];
        // translations
        if (Translations != null)
        {
            Directory.CreateDirectory(translationsDir);
            bool translationRequiresLoad = false;
            foreach (IEditableAsset editable in EditableAssets)
            {
                translationRequiresLoad =
                    editable.GetTranslations(ref Translations, manifest.Name) || translationRequiresLoad;
            }
            if (translationRequiresLoad)
            {
                changes.Add(
                    new MockLoad(TranslationString.I18N_Asset, Path.Combine("i18n", TranslationStore.DefaultFilename))
                    {
                        Priority = AssetLoadPriority.Low.ToString(),
                    }
                );
                changes.Add(
                    new MockLoad(TranslationString.I18N_Asset, "i18n/{{Language}}.json")
                    {
                        When = new() { ["HasFile:{{FromFile}}"] = true },
                    }
                );
            }
            // i18n/{langaugecode}.json and i18n/default.json
            ModEntry.WriteJson(translationsDir, Translations.LocaleFilename, Translations.Data);
            ModEntry.WriteJson(translationsDir, TranslationStore.DefaultFilename, Translations.DefaultData);
        }
        // edits
        HashSet<IAssetName> requiredAssets = [];
        foreach (IEditableAsset editable in EditableAssets)
        {
            List<IMockPatch> patches = editable.GetPatches(this).ToList();
            if (patches.Count == 0)
            {
                File.Delete(Path.Combine(dataDir, editable.IncludeName));
                continue;
            }
            changes.Add(new MockInclude(Path.Combine("data", editable.IncludeName)));
            ModEntry.WriteJson(dataDir, editable.IncludeName, new MockContent(patches));
            requiredAssets.AddRange(editable.GetRequiredAssets());
        }
        // loads
        foreach (ILoadableAsset loadable in LoadableAssets)
        {
            if (
                loadable.StageAndGetTargetAndFromFile(assetsDir, ref requiredAssets)
                is ValueTuple<string, string> result
            )
            {
                changes.Add(new MockLoad(result.Item1, Path.Join("assets", result.Item2)));
            }
        }

        List<string> includeList = [];
        foreach (IMockPatch mockPatch in changes)
        {
            if ((mockPatch.When?.TryGetValue("HasMod", out object? maybeModId) ?? false) && maybeModId is string modId)
            {
                manifest.OptionalDependencies.Add(modId);
            }
            if (mockPatch is MockInclude inc)
            {
                includeList.Add(inc.FromFile);
            }
        }
        manifest.StardewModSmithyInfo = new([Consts.MANIFEST_FILE, "i18n/*.json", "content.json", .. includeList]);

        // content.json
        ModEntry.WriteJson(targetPath, "content.json", new MockContentMain(changes));
        // manifest.json
        ModEntry.WriteJson(targetPath, Consts.MANIFEST_FILE, manifest);

        ModEntry.PatchReload(targetPath, manifest.UniqueID);
    }

    public void Load()
    {
        string targetPath = manifest.OutputFolder;

        string dataDir = Path.Combine(targetPath, "data");
        if (!Directory.Exists(dataDir))
            return;

        foreach (string file in Directory.GetFiles(dataDir))
        {
            if (!file.EndsWith(".json"))
            {
                continue;
            }
            string fileName = Path.GetFileName(file);
            if (
                ModEntry.ReadJson<MockContentEditDataInclude>(dataDir, fileName)
                    is not MockContentEditDataInclude editDataContent
                || editDataContent.Changes.FirstOrDefault(patch => patch is not null) is not MockEditData editData
            )
            {
                continue;
            }
            switch (fileName)
            {
                case FurnitureAsset.DEFAULT_INCLUDE_NAME:
                    InitializeFurnitureAsset(editData.Entries);
                    break;
                case WallpaperFlooringAsset.DEFAULT_INCLUDE_NAME:
                    InitializeWallpaperAndFlooringAsset(editData.Entries);
                    break;
            }
            continue;
        }
    }

    public void InitializeFurnitureAsset(Dictionary<string, object> entries)
    {
        FurniAsset = new();
        FurniAsset.SetData(entries);
        FurniAsset.SetTranslations(Translations);
    }

    public void InitializeWallpaperAndFlooringAsset(Dictionary<string, object> entries)
    {
        WallAndFloorAsset = new();
        WallAndFloorAsset.SetData(entries);
        WallAndFloorAsset.SetTranslations(Translations);
    }
}
