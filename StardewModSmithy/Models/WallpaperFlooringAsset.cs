using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley.GameData;

namespace StardewModSmithy.Models;

public sealed record EditableWallpaperOrFlooring(ModWallpaperOrFlooring BaseData) : IBoundsProvider
{
    private const int WF_WIDTH = 256;
    private const int PER_ROW_FLOOR_COUNT = WF_WIDTH / 32;
    private const int PER_ROW_WALL_COUNT = WF_WIDTH / 16;

    public IAssetName? TextureAssetName { get; set; } = ModEntry.ParseAssetName(BaseData.Texture);

    public int SpriteIndex => 0;

    public Point TilesheetSize
    {
        get
        {
            if (BaseData.IsFlooring)
            {
                ModEntry.Log(
                    new Point(
                        2 * Math.Min(BaseData.Count, PER_ROW_FLOOR_COUNT),
                        2 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_FLOOR_COUNT)
                    ).ToString()
                );
                return new(
                    2 * Math.Min(BaseData.Count, PER_ROW_FLOOR_COUNT),
                    2 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_FLOOR_COUNT)
                );
            }
            else
            {
                ModEntry.Log(
                    new Point(
                        1 * Math.Min(BaseData.Count, PER_ROW_WALL_COUNT),
                        3 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_WALL_COUNT)
                    ).ToString()
                );
                return new(
                    Math.Min(BaseData.Count, PER_ROW_WALL_COUNT),
                    3 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_WALL_COUNT)
                );
            }
        }
    }

    public Point BoundingBoxSize
    {
        get
        {
            if (BaseData.IsFlooring)
            {
                return new(2, 2);
            }
            else
            {
                return new(1, 3);
            }
        }
    }

    public string GUI_TilesheetArea => Consts.Basic_GUI_TilesheetSize(TilesheetSize);

    public IEnumerable<SDUIEdges> GUI_BoundingSquares =>
        Consts.Basic_GUI_BoundingSquares(TilesheetSize, BoundingBoxSize);

    public string UILabel => BaseData.Id;
}

public sealed class WallpaperFlooringAsset : IEditableAsset
{
    public const string DEFAULT_INCLUDE_NAME = "wallpaper_flooring.json";
    public const string TARGET_ASSET = "Data/AdditionalWallpaperFlooring";

    public string Desc => "wallpaper and flooring";
    public string IncludeName => DEFAULT_INCLUDE_NAME;

    public Dictionary<string, EditableWallpaperOrFlooring> Editing = [];

    public IEnumerable<IMockPatch> GetPatches()
    {
        Dictionary<string, object> output = [];
        foreach (EditableWallpaperOrFlooring wallfloor in Editing.Values)
        {
            if (wallfloor.TextureAssetName != null)
                output[string.Concat(Sanitize.ModIdPrefixValue, wallfloor.BaseData.Id)] = wallfloor.BaseData;
        }
        yield return new MockEditData(TARGET_ASSET, output);
    }

    public IEnumerable<IAssetName> GetRequiredAssets() => Editing.Values.GetRequiredAssetsFromIBoundsProvider();

    public bool GetTranslations(ref TranslationStore translations, string modName)
    {
        return false;
    }

    public void SetData(Dictionary<string, object> data)
    {
        foreach ((string key, object value) in data)
        {
            if (value is not ModWallpaperOrFlooring wallfloor)
                return;
            string baseKey = Sanitize.ModIdPrefix(key);
            Editing[baseKey] = new(wallfloor);
        }
    }

    public void SetTranslations(TranslationStore? translations)
    {
        throw new NotImplementedException();
    }
}
