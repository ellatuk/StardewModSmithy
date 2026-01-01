using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models.ValueKinds;

public enum TranslationStringKind
{
    LocalizedText = 0,
    ContentPatcherI18N = 1,
}

public sealed class TranslationString(string key)
{
    public const string I18N_Asset = "{{ModId}}.i18n";
    public TranslationStringKind Kind = TranslationStringKind.LocalizedText;
    public string Key { get; set; } = Sanitize.Key(key);
    public string? Value { get; set; } = null;

    public static readonly Regex localizedTextRE = new(@"\[LocalizedText\s+{{ModId}}\.i18n:([^\s]+)\]");
    public static readonly Regex contentPatcherI18NRE = new(@"\{\{i18n:\s*(.+)\s+\}\}");

    public static TranslationString? Deserialize(string str)
    {
        if (localizedTextRE.Match(str) is Match match1 && match1.Success)
        {
            return new TranslationString(match1.Groups[1].Value) { Kind = TranslationStringKind.LocalizedText };
        }
        else if (contentPatcherI18NRE.Match(str) is Match match2 && match2.Success)
        {
            return new TranslationString(match2.Groups[1].Value) { Kind = TranslationStringKind.ContentPatcherI18N };
        }
        return null;
    }

    internal void SetValueFrom(TranslationStore translations)
    {
        if (translations.Data.TryGetValue(Key, out string? value))
        {
            Value = value;
        }
    }

    public string GetToken()
    {
        return Kind switch
        {
            TranslationStringKind.LocalizedText => $"[LocalizedText {I18N_Asset}:{Key}]",
            TranslationStringKind.ContentPatcherI18N => $"{{{{i18n: {Key}}}}}",
            _ => Value ?? Key,
        };
    }
}
