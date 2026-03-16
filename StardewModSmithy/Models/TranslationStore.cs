using StardewModSmithy.Wheels;
using StardewValley;
#if SDV17
using StardewValley.ContentManagement;
#else
using LanguageCode = StardewValley.LocalizedContentManager.LanguageCode;
#endif

namespace StardewModSmithy.Models;

public sealed class TranslationStore(string translationsDir)
{
    public const string DefaultFilename = "default.json";
#if SDV17
    public static LanguageCode Code => Game1.content.LanguageCode;
#else
    public static LanguageCode Code => Game1.content.GetCurrentLanguage();
#endif
    public Dictionary<LanguageCode, Dictionary<string, string>> PerLang = LoadI18NData(translationsDir);
    public Dictionary<string, string> Data => PerLang[Code];
    public Dictionary<string, string> DefaultData =
        translationsDir != null
            ? ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, DefaultFilename) ?? []
            : [];

    public static Dictionary<LanguageCode, Dictionary<string, string>> LoadI18NData(string translationsDir)
    {
        Dictionary<LanguageCode, Dictionary<string, string>> perLang = [];
        foreach (LanguageCode lang in Enum.GetValues<LanguageCode>())
        {
            perLang[lang] =
                ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, string.Concat(lang, ".json")) ?? [];
        }
        return perLang;
    }

    internal void SetDataKeyValue(string key, string value, bool overwrite = true)
    {
        if (overwrite || !Data.ContainsKey(key))
            Data[key] = value;
        if (!DefaultData.ContainsKey(key))
        {
            DefaultData[key] = value.Equals(Utils.DEFAULT_STR) ? key : value;
        }
    }

    internal void WriteI18NData()
    {
        Directory.CreateDirectory(translationsDir);
        // i18n/default.json
        if (DefaultData.Any())
            ModEntry.WriteJson(translationsDir, DefaultFilename, DefaultData);
        // i18n/{langaugecode}.json
        foreach ((LanguageCode lang, Dictionary<string, string> data) in PerLang)
        {
            if (data.Any())
                ModEntry.WriteJson(translationsDir, string.Concat(lang, ".json"), data);
        }
    }
}
