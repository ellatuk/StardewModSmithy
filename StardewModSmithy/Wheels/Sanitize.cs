using System.Buffers.Text;
using System.Text;
using System.Text.RegularExpressions;
using StardewModdingAPI;

namespace StardewModSmithy.Wheels;

internal static class Sanitize
{
    public const string ModIdPrefixValue = "{{ModId}}_";

    public static string SanitizeImpl(string value, char? replacement, char[] illegal)
    {
        if (replacement == null)
            return string.Join("", value.Split(illegal));
        return string.Join(replacement.Value, value.Split(illegal));
    }

    public static Regex UniqueIDPattern = new(@"^[^a-zA-Z0-9_.-]+$");

    public static readonly char[] IllegalKeyChars = ['{', '}', '[', ']', '(', ')', ':', '/', ',', ' '];

    public static string Key(string key)
    {
        return SanitizeImpl(key, '.', IllegalKeyChars);
    }

    public static string Path(string path)
    {
        return SanitizeImpl(path, '_', System.IO.Path.GetInvalidFileNameChars());
    }

    public static string UniqueID(string id)
    {
        string result = UniqueIDPattern.Replace(id, string.Empty);
        if (string.IsNullOrEmpty(result))
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(id));
        }
        return result;
    }

    public static string AssetName(IAssetName assetName)
    {
        return assetName.BaseName.Replace('/', '\\');
    }

    public static string ModIdPrefix(string name)
    {
        if (name.StartsWith(ModIdPrefixValue))
        {
            return name.Replace(ModIdPrefixValue, "");
        }
        return name;
    }
}
