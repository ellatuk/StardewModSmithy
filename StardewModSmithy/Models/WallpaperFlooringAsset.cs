using System.ComponentModel;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewModSmithy.Models;

public sealed record EditableWallpaperOrFlooring(string BaseKey, ModWallpaperOrFlooring BaseData)
    : IBoundsProvider,
        INotifyPropertyChanged
{
    private const int WF_WIDTH = 256;
    private const int PER_ROW_FLOOR_COUNT = WF_WIDTH / 32;
    private const int PER_ROW_WALL_COUNT = WF_WIDTH / 16;

    public IAssetName? TextureAssetName
    {
        get => ModEntry.ParseAssetName(BaseData.Texture);
        set
        {
            if (value != null)
                BaseData.Texture = value.BaseName;
        }
    }

    public int SpriteIndex => 0;

    public Point TilesheetSize
    {
        get
        {
            if (BaseData.IsFlooring)
            {
                return new(
                    2 * Math.Min(BaseData.Count, PER_ROW_FLOOR_COUNT),
                    2 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_FLOOR_COUNT)
                );
            }
            else
            {
                return new(
                    Math.Min(BaseData.Count, PER_ROW_WALL_COUNT),
                    3 * (int)Math.Ceiling(BaseData.Count / (float)PER_ROW_WALL_COUNT)
                );
            }
        }
    }

    public Point BoundingBoxSize => BaseData.IsFlooring ? new(2, 2) : new(1, 3);

    public string GUI_TilesheetArea => Consts.Basic_GUI_TilesheetSize(TilesheetSize);

    public IEnumerable<SDUIEdges> GUI_BoundingSquares
    {
        get
        {
            int perRowCount;
            if (BaseData.IsFlooring)
            {
                perRowCount = (BaseData.Count - 1) % PER_ROW_FLOOR_COUNT;
                perRowCount *= 2;
            }
            else
            {
                perRowCount = (BaseData.Count - 1) % PER_ROW_WALL_COUNT;
            }
            for (int x = 0; x < BoundingBoxSize.X; x++)
            {
                for (int y = 0; y < BoundingBoxSize.Y; y++)
                {
                    yield return new(
                        (perRowCount + x) * Consts.DRAW_TILE,
                        (TilesheetSize.Y - 1 - y) * Consts.DRAW_TILE
                    );
                }
            }
        }
    }

    public string UILabel => BaseKey;

    public bool IsFlooring
    {
        get => BaseData.IsFlooring;
        set
        {
            BaseData.IsFlooring = value;
            OnPropertyChanged(new(nameof(IsFlooring)));
            OnPropertyChanged(new(nameof(GUI_BoundingSquares)));
            OnPropertyChanged(new(nameof(GUI_TilesheetArea)));
        }
    }

    public IntSpinBoxViewModel Count =>
        new(
            () => BaseData.Count,
            (value) =>
            {
                BaseData.Count = value;
                OnPropertyChanged(new(nameof(BoundsLabel)));
                OnPropertyChanged(new(nameof(GUI_BoundingSquares)));
                OnPropertyChanged(new(nameof(GUI_TilesheetArea)));
            },
            1,
            int.MaxValue
        );

    public string BoundsLabel => BaseData.Count.ToString();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(PropertyChangedEventArgs value) => PropertyChanged?.Invoke(this, value);
}

public sealed class WallpaperFlooringAsset : IEditableAsset
{
    public const string DEFAULT_INCLUDE_NAME = "wallpaper_flooring.json";
    public const string TARGET_ASSET = "Data/AdditionalWallpaperFlooring";

    public string IncludeName => DEFAULT_INCLUDE_NAME;

    public Dictionary<string, EditableWallpaperOrFlooring> Editing = [];

    public IEnumerable<IMockPatch> GetPatches(IOutputPack outputPack)
    {
        Dictionary<string, object> output = [];
        foreach (EditableWallpaperOrFlooring wallfloor in Editing.Values)
        {
            if (wallfloor.TextureAssetName != null)
            {
                string fullId = string.Concat(Sanitize.ModIdPrefixValue, wallfloor.BaseData.Id);
                wallfloor.BaseData.Id = fullId;
                output[fullId] = wallfloor.BaseData;
            }
        }
        if (output.Any())
            yield return new MockEditData(TARGET_ASSET, output);
    }

    public bool GetTranslations(ref TranslationStore translations, string modName) => false;

    public void SetTranslations(TranslationStore? translations) { }

    public IEnumerable<IAssetName> GetRequiredAssets() => Editing.Values.GetRequiredAssetsFromIBoundsProvider();

    public void SetData(Dictionary<string, object> data)
    {
        foreach ((string key, object value) in data)
        {
            if (JToken.FromObject(value).ToObject<ModWallpaperOrFlooring>() is not ModWallpaperOrFlooring wallfloor)
                return;
            string baseKey = Sanitize.ModIdPrefix(key);
            wallfloor.Id = baseKey;
            Editing[baseKey] = new(baseKey, wallfloor);
        }
    }

    public static bool TextureFilter(TextureAsset asset)
    {
        return asset.Texture.Width == 256;
    }

    public EditableWallpaperOrFlooring? AddNewDefault(IAssetName assetName)
    {
        string id = Sanitize.Key(Path.GetFileNameWithoutExtension(assetName.BaseName));
        if (Editing.ContainsKey(id))
            return null;
        ModWallpaperOrFlooring newWallFloor = new()
        {
            Id = id,
            Texture = assetName.BaseName,
            IsFlooring = false,
            Count = 1,
        };
        EditableWallpaperOrFlooring newWallFloorEdit = new(newWallFloor.Id, newWallFloor);
        Editing[newWallFloor.Id] = newWallFloorEdit;
        return newWallFloorEdit;
    }

    internal bool Delete(EditableWallpaperOrFlooring selectedFurniture)
    {
        return Editing.RemoveWhere(kv => kv.Value.BaseData.Id == selectedFurniture.BaseData.Id) > 0;
    }
}
