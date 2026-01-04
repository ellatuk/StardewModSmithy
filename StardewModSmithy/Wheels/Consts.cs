using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;

namespace StardewModSmithy.Wheels;

internal static class Consts
{
    internal const int TX_TILE = 16;
    public const int DRAW_TILE = 64;
    internal const string EDITING_INPUT = "editing/input";
    internal const string EDITING_OUTPUT = "editing/output";
    internal const string MANIFEST_FILE = "manifest.json";
    internal const string ATLAS_SUFFIX = ".atlas.json";

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

    internal static IEnumerable<SDUIEdges> Basic_GUI_BoundingSquares(Point TilesheetSize, Point BoundingBoxSize)
    {
        for (int x = 0; x < BoundingBoxSize.X; x++)
        {
            for (int y = 0; y < BoundingBoxSize.Y; y++)
            {
                yield return new(x * Consts.DRAW_TILE, (TilesheetSize.Y - 1 - y) * Consts.DRAW_TILE);
            }
        }
    }
}
