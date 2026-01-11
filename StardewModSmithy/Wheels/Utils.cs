using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;

namespace StardewModSmithy.Wheels;

internal static class Utils
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
    internal const string DEFAULT_STR = "???";

    public static bool StageByCopy => Constants.TargetPlatform == GamePlatform.Windows;

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

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    internal static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
