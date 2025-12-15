using StardewValley;

namespace StardewModSmithy.Models;

public sealed class TranslationStore
{
    public const string DefaultFilename = "default.json";
    public LocalizedContentManager.LanguageCode code = Game1.content.GetCurrentLanguage();
    public string LocaleFilename => $"{code}.json";
    public Dictionary<string, string> Data = [];

    public void LoadForCurrentLanguage(string translationsDir)
    {
        code = Game1.content.GetCurrentLanguage();
        Data =
            ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, LocaleFilename)
            ?? ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, DefaultFilename)
            ?? Data;
    }

    public static TranslationStore? FromSourceDir(string translationsDir)
    {
        if (!Directory.Exists(translationsDir))
        {
            return null;
        }
        TranslationStore store = new();
        store.LoadForCurrentLanguage(translationsDir);
        return store;
    }
}
