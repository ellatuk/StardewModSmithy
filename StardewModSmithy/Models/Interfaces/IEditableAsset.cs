using StardewModdingAPI;

namespace StardewModSmithy.Models.Interfaces;

public interface IEditableAsset
{
    public string Desc { get; }
    public string IncludeName { get; }
    public IEnumerable<IMockPatch> GetPatches();
    public void SetData(Dictionary<string, object> data);
    public IEnumerable<IAssetName> GetRequiredAssets();
    public bool GetTranslations(ref TranslationStore translations);
    public void SetTranslations(TranslationStore? translations);
}
