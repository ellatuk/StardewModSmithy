using StardewModdingAPI;

namespace StardewModSmithy.Models.Interfaces;

public interface IEditableAsset
{
    public string IncludeName { get; }
    public IEnumerable<IMockPatch> GetPatches();
    public void SetData(Dictionary<string, object> data);
    public IEnumerable<IAssetName> GetRequiredAssets();
    public bool GetTranslations(ref TranslationStore translations, string modName);
    public void SetTranslations(TranslationStore? translations);
}
