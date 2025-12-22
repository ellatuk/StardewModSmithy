using StardewModdingAPI;

namespace StardewModSmithy.Models.Interfaces;

public interface IEditableAsset
{
    public string Desc { get; }
    public string Target { get; }
    public string IncludeName { get; }
    public Dictionary<string, object> GetData();
    public IEnumerable<IAssetName> GetRequiredAssets();
    public bool GetTranslations(ref TranslationStore translations);
    public void SetTranslations(TranslationStore? translations);
}
