using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;

namespace StardewModSmithy.Wheels;

internal static class Consts
{
    internal const int TX_TILE = 16;
    public const int DRAW_TILE = 64;
    internal const string EDITING_INPUT = "editing/input";
    internal const string EDITING_OUTPUT = "editing/output";
    internal const string MANIFEST_FILE = "manifest.json";
    internal const string ATLAS_SUFFIX = ".atlas.json";
    internal const string TL_DIR = "i18n";
    internal const string ASSETS_DIR = "assets";
    internal const string DATA_DIR = "data";
    internal const string CUSTOM_DIR = "custom";

    internal static IEnumerable<IAssetName> GetRequiredAssetsFromIBoundsProvider(
        this IEnumerable<IBoundsProvider> boundsProviders
    )
    {
        foreach (IBoundsProvider bp in boundsProviders)
        {
            if (bp.TextureAssetName != null)
                yield return bp.TextureAssetName;
        }
    }

    internal static string Basic_GUI_TilesheetSize(Point TilesheetSize)
    {
        return $"{TilesheetSize.X * DRAW_TILE}px {TilesheetSize.Y * DRAW_TILE}px";
    }

    internal static (int, string) GetSeq(Func<string, bool> contains)
    {
        int seq = 0;
        string seqId = seq.ToString();
        while (contains(seqId))
        {
            seq++;
            seqId = seq.ToString();
        }
        return new(seq, seqId);
    }

    public static void BrowseFolder(string path, bool createIfNotExist = true)
    {
        if (Directory.Exists(ModEntry.OutputDirectoryPath))
        {
            if (!Directory.Exists(path))
            {
                if (!createIfNotExist)
                    return;
                Directory.CreateDirectory(path);
            }
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true,
                        Verb = "open",
                    }
                );
            }
            catch (Exception err)
            {
                ModEntry.Log($"Failed to open '{path}'\n{err}", LogLevel.Error);
            }
        }
    }
}
