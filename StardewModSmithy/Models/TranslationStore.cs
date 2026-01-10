using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.Models;

public sealed class TranslationStore(string translationsDir)
{
    public const string DefaultFilename = "default.json";
    public static LocalizedContentManager.LanguageCode Code => Game1.content.GetCurrentLanguage();
    public Dictionary<LocalizedContentManager.LanguageCode, Dictionary<string, string>> PerLang = LoadI18NData(
        translationsDir
    );
    public Dictionary<string, string> Data => PerLang[Code];
    public Dictionary<string, string> DefaultData =
        translationsDir != null
            ? ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, DefaultFilename) ?? []
            : [];

    public static Dictionary<LocalizedContentManager.LanguageCode, Dictionary<string, string>> LoadI18NData(
        string translationsDir
    )
    {
        Dictionary<LocalizedContentManager.LanguageCode, Dictionary<string, string>> perLang = [];
        foreach (LocalizedContentManager.LanguageCode lang in Enum.GetValues<LocalizedContentManager.LanguageCode>())
        {
            perLang[lang] =
                ModEntry.ReadJson<Dictionary<string, string>>(translationsDir, string.Concat(lang, ".json")) ?? [];
        }
        return perLang;
    }

    internal void SetDataKeyValue(string key, string value, bool overwrite = true)
    {
        ModEntry.Log($"Code {Code}");
        if (overwrite || !Data.ContainsKey(key))
            Data[key] = value;
        if (!DefaultData.ContainsKey(key))
        {
            DefaultData[key] = value.Equals(Consts.DEFAULT_STR) ? key : value;
        }
    }

    internal void WriteI18NData()
    {
        Directory.CreateDirectory(translationsDir);
        // i18n/{langaugecode}.json and i18n/default.json
        ModEntry.WriteJson(translationsDir, DefaultFilename, DefaultData);
        foreach ((LocalizedContentManager.LanguageCode lang, Dictionary<string, string> data) in PerLang)
        {
            if (data.Any())
                ModEntry.WriteJson(translationsDir, string.Concat(lang, ".json"), data);
        }
    }
}
