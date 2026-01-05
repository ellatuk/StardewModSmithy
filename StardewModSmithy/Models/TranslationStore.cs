using StardewValley;

namespace StardewModSmithy.Models;

public sealed class TranslationStore
{
    public const string DefaultFilename = "default.json";
    public LocalizedContentManager.LanguageCode code = Game1.content.GetCurrentLanguage();
    public string LocaleFilename => $"{code}.json";
    public Dictionary<string, string> Data = [];
    public Dictionary<string, string> DefaultData = [];

    public void LoadForCurrentLanguage(string translationsDir)
    {
        code = Game1.content.GetCurrentLanguage();
        DefaultData = ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, DefaultFilename) ?? DefaultData;
        Data = ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, DefaultFilename) ?? Data;
    }

    public static TranslationStore? FromSourceDir(string translationsDir)
    {
        if (!Directory.Exists(translationsDir))
        {
            return new();
        }
        TranslationStore store = new();
        store.LoadForCurrentLanguage(translationsDir);
        return store;
    }

    internal void SetDataKeyValue(string key, string v)
    {
        Data[key] = v;
        if (!DefaultData.ContainsKey(key))
        {
            DefaultData[key] = v;
        }
    }
}
