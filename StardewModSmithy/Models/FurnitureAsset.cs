using System.Text;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;
using StardewModSmithy.Wheels;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Shops;
using xTile.Layers;

namespace StardewModSmithy.Models;

public sealed partial class FurnitureDelimString(string id) : IBoundsProvider
{
    public const char DELIM = '/';
    public const string CATALOGUE_CTAG = "stardew_mod_smithy_catalogue";
    #region options
    private static readonly string[] type_Options =
    [
        "armchair",
        "bench",
        "chair",
        "couch",
        "long table",
        "table",
        "fireplace",
        "lamp",
        "sconce",
        "torch",
        "window",
        "bed",
        "bed double",
        "bed child",
        "dresser",
        "fishtank",
        "bookcase",
        "decor",
        "painting",
        "randomized_plant",
        "rug",
        "other",
    ];
    private static readonly int[] rotation_Options = [1, 2, 4];
    private static readonly int[] placement_Options = [-1, 0, 1, 2];
#pragma warning disable CA1822 // Mark members as static
    public string[] Type_Options => type_Options;
    public int[] Rotation_Options => rotation_Options;
    public int[] Placement_Options => placement_Options;
#pragma warning restore CA1822 // Mark members as static
    #endregion

    #region fields
    public string Id = id;
    public string Name = id;

    public OptionedValue<string> TypeImpl { get; } = new(type_Options, "decor");
    public string Type
    {
        get => TypeImpl.Value;
        set
        {
            TypeImpl.Value = value;
            OnPropertyChanged(new(nameof(Type)));
        }
    }

    [Notify]
    public Point tilesheetSize = Point.Zero;
    public string TilesheetSizeName => $"{TilesheetSize.X} {TilesheetSize.Y}";
    public IntSpinBoxViewModel TilesheetSizeX =>
        new(() => TilesheetSize.X, (value) => TilesheetSize = new(value, TilesheetSize.Y), 1, int.MaxValue);
    public IntSpinBoxViewModel TilesheetSizeY =>
        new(() => TilesheetSize.Y, (value) => TilesheetSize = new(TilesheetSize.X, value), 1, int.MaxValue);

    [Notify]
    public Point boundingBoxSize = Point.Zero;
    public string BoundingBoxSizeName => $"{BoundingBoxSize.X} {BoundingBoxSize.Y}";
    public IntSpinBoxViewModel BoundingBoxSizeX =>
        new(() => BoundingBoxSize.X, (value) => BoundingBoxSize = new(value, BoundingBoxSize.Y), 1, int.MaxValue);
    public IntSpinBoxViewModel BoundingBoxSizeY =>
        new(() => BoundingBoxSize.Y, (value) => BoundingBoxSize = new(BoundingBoxSize.X, value), 1, int.MaxValue);

    public string GUI_TilesheetArea => Consts.Basic_GUI_TilesheetSize(TilesheetSize);

    public IEnumerable<SDUIEdges> GUI_BoundingSquares =>
        Consts.Basic_GUI_BoundingSquares(TilesheetSize, BoundingBoxSize);

    private readonly OptionedValue<int> RotationImpl = new(rotation_Options, 1);
    public int Rotation
    {
        get => (int)RotationImpl.Value;
        set
        {
            RotationImpl.Value = value;
            OnPropertyChanged(new(nameof(Rotation)));
        }
    }
    public Func<int, string> RotationName = (rot) => I18n.GetByKey($"gui.rotation.{rot}.name");

    [Notify]
    public int price = 0;

    public string PriceInput
    {
        get => Price.ToString();
        set
        {
            if (int.TryParse(value, out int newPrice))
            {
                Price = newPrice;
            }
            else
            {
                OnPropertyChanged(new(nameof(Price)));
            }
        }
    }

    private readonly OptionedValue<int> PlacementImpl = new(placement_Options, 2);
    public int Placement
    {
        get => (int)PlacementImpl.Value;
        set
        {
            PlacementImpl.Value = value;
            OnPropertyChanged(new(nameof(Placement)));
        }
    }
    public Func<int, string> PlacementName = (place) =>
        place == -1 ? I18n.Gui_Placement_Neg1_Name() : I18n.GetByKey($"gui.placement.{place}.name");

    public TranslationString DisplayNameImpl { get; private set; } = new(string.Concat(id, ".name"));
    public string DisplayName
    {
        get => DisplayNameImpl.Value ?? "???";
        set
        {
            DisplayNameImpl.Value = value;
            OnPropertyChanged(new(nameof(DisplayName)));
        }
    }

    public int SpriteIndex { get; set; } = 0;

    public IAssetName? TextureAssetName { get; set; }

    [Notify]
    public bool offLimitsForRandomSale = false;
    public HashSet<string> ContextTags { get; set; } = [];

    [Notify]
    private bool isCatalogue = false;

    #endregion

    public string UILabel => $"{Id}:{DisplayName}";

    internal bool FromDeserialize = false;
    internal int PreSerializeSeq = -1;

    private static Point PointFromString(string str)
    {
        string[] parts = str.Split(' ');
        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
        {
            return new(x, y);
        }
        return Point.Zero;
    }

    private static string PointToString(Point point)
    {
        return string.Concat(point.X, ' ', point.Y);
    }

    public static FurnitureDelimString? Deserialize(string id, string str)
    {
        string[] parts = str.Split(DELIM);
        if (parts.Length < 9)
        {
            ModEntry.Log($"Not enough '{DELIM}' delim parts {str}", LogLevel.Warn);
            return null;
        }

        FurnitureDelimString furniDelimString = new(id)
        {
            Name = Sanitize.ModIdPrefix(parts[0]),
            Type = parts[1],
            TilesheetSize = PointFromString(parts[2]),
            BoundingBoxSize = PointFromString(parts[3]),
        };

        if (int.TryParse(parts[4], out int rotation))
        {
            furniDelimString.Rotation = rotation;
        }
        if (int.TryParse(parts[5], out int price))
        {
            furniDelimString.Price = price;
        }
        if (int.TryParse(parts[6], out int placement))
        {
            furniDelimString.Placement = placement;
        }
        if (TranslationString.Deserialize(parts[7]) is TranslationString displayName)
        {
            furniDelimString.DisplayNameImpl = displayName;
        }
        else
        {
            furniDelimString.DisplayName = parts[7];
        }
        if (int.TryParse(parts[8], out int spriteIndex))
        {
            furniDelimString.SpriteIndex = spriteIndex;
        }

        if (parts.Length > 9)
            furniDelimString.TextureAssetName = ModEntry.ParseAssetName(parts[9]);
        if (parts.Length > 10 && bool.TryParse(parts[10], out bool offlim))
            furniDelimString.OffLimitsForRandomSale = offlim;
        if (parts.Length > 11)
        {
            furniDelimString.ContextTags = parts[11].Split(' ').ToHashSet();
            furniDelimString.IsCatalogue = furniDelimString.ContextTags.Contains(CATALOGUE_CTAG);
        }

        furniDelimString.FromDeserialize = true;
        return furniDelimString;
    }

    private static readonly StringBuilder sb = new();

    public void UpdateForFirstTimeSerialize()
    {
        if (TextureAssetName == null || FromDeserialize)
            return;

        Id = string.Concat(Sanitize.Key(Path.GetFileName(TextureAssetName.BaseName)), '_', Id);
        Name = Id;
        DisplayNameImpl.Key = string.Concat(Id, ".name");
        FromDeserialize = true;
    }

    public string Serialize()
    {
        if (TextureAssetName == null)
            return string.Empty;

        sb.Append(string.Concat(Sanitize.ModIdPrefixValue, Name));
        sb.Append(DELIM);

        sb.Append(Type);
        sb.Append(DELIM);

        sb.Append(PointToString(TilesheetSize));
        sb.Append(DELIM);

        sb.Append(PointToString(BoundingBoxSize));
        sb.Append(DELIM);

        sb.Append(Rotation);
        sb.Append(DELIM);

        sb.Append(Price);
        sb.Append(DELIM);

        sb.Append(Placement);
        sb.Append(DELIM);

        sb.Append(DisplayNameImpl.GetToken());
        sb.Append(DELIM);

        sb.Append(SpriteIndex);
        sb.Append(DELIM);

        sb.Append(string.Concat(TextureAssetName.BaseName.Replace(DELIM, '\\')));
        sb.Append(DELIM);

        sb.Append(OffLimitsForRandomSale);
        sb.Append(DELIM);

        if (IsCatalogue)
        {
            ContextTags ??= [];
            ContextTags.Add(CATALOGUE_CTAG);
        }
        else
        {
            ContextTags?.Remove(CATALOGUE_CTAG);
        }

        if (ContextTags != null)
        {
            sb.AppendJoin(' ', ContextTags.OrderBy(value => value));
        }

        string result = sb.ToString();
        sb.Clear();
        return result;
    }
}

public sealed class FurnitureAsset : IEditableAsset
{
    public const string DEFAULT_INCLUDE_NAME = "furniture.json";
    public string Desc => "furniture";
    public string IncludeName => DEFAULT_INCLUDE_NAME;
    public Dictionary<string, FurnitureDelimString> Editing = [];

    public IEnumerable<IMockPatch> GetPatches()
    {
        Dictionary<string, object> output = [];
        Dictionary<string, FurnitureDelimString> catalogue = [];
        foreach (FurnitureDelimString furniDelim in Editing.Values)
        {
            if (furniDelim.TextureAssetName != null)
            {
                string fullId = string.Concat(Sanitize.ModIdPrefixValue, furniDelim.Id);
                output[fullId] = furniDelim.Serialize();
                if (furniDelim.IsCatalogue)
                {
                    catalogue[fullId] = furniDelim;
                }
            }
        }
        yield return new MockEditData("Data/Furniture", output);
        if (catalogue.Any())
        {
            Dictionary<string, object> hasMMAP = new() { ["HasMod"] = "mushymato.MMAP" };
            yield return new MockEditData(
                "Data/Shops",
                new Dictionary<string, object>()
                {
                    ["{{ModId}}_furniture_catalogue"] = new
                    {
                        Items = new List<object>
                        {
                            new
                            {
                                Id = "{{ModId}}_catalogue_all_furniture",
                                ItemId = "ALL_ITEMS (F)",
                                PerItemCondition = "ITEM_ID_PREFIX Target {{ModId}}_",
                            },
                        },
                        CustomFields = new Dictionary<string, string>()
                        {
                            ["HappyHomeDesigner/Catalogue"] = true.ToString(),
                        },
                    },
                }
            )
            {
                When = hasMMAP,
            };
            List<object> catalogueShopItems = [];
            Dictionary<string, object> catalogueTileProp = [];
            foreach ((string itemId, FurnitureDelimString furni) in catalogue)
            {
                string qId = string.Concat("(F)", itemId);
                catalogueShopItems.Add(new { Id = qId, ItemId = qId });
                catalogueTileProp[itemId] = new
                {
                    TileProperties = new List<object>
                    {
                        new
                        {
                            Id = "OpenShop {{ModId}}_furniture_catalogue",
                            Name = "Action",
                            Value = "OpenShop {{ModId}}_furniture_catalogue",
                            Layer = "Buildings",
                            TileArea = new Rectangle(Point.Zero, furni.boundingBoxSize),
                        },
                    },
                };
            }

            yield return new MockEditData(
                "Data/Shops",
                new Dictionary<string, object>()
                {
                    ["{{ModId}}_furniture_catalogue"] = new
                    {
                        Items = catalogueShopItems,
                        CustomFields = new Dictionary<string, string>()
                        {
                            ["HappyHomeDesigner/Catalogue"] = true.ToString(),
                        },
                    },
                }
            )
            {
                TargetField = ["Carpenter", "Items"],
                When = hasMMAP,
            };

            yield return new MockEditData("mushymato.MMAP/FurnitureProperties", catalogueTileProp) { When = hasMMAP };
        }
    }

    public void SetData(Dictionary<string, object> data)
    {
        foreach ((string key, object value) in data)
        {
            if (value is not string strV)
                return;
            string baseKey = Sanitize.ModIdPrefix(key);
            if (FurnitureDelimString.Deserialize(baseKey, strV) is FurnitureDelimString furniDelim)
                Editing[baseKey] = furniDelim;
        }
    }

    public FurnitureDelimString AddNewDefault()
    {
        int seq = 0;
        string seqId = seq.ToString();
        while (Editing.ContainsKey(seqId))
        {
            seq++;
            seqId = seq.ToString();
        }
        FurnitureDelimString newDefaultFurni = new(seqId)
        {
            Name = seqId,
            Type = "decor",
            TilesheetSize = new(1, 1),
            BoundingBoxSize = new(1, 1),
            PreSerializeSeq = seq,
        };
        Editing[seqId] = newDefaultFurni;
        return newDefaultFurni;
    }

    internal bool Delete(FurnitureDelimString selectedFurniture)
    {
        return Editing.RemoveWhere(kv => kv.Value.Id == selectedFurniture.Id) > 0;
    }

    public IEnumerable<IAssetName> GetRequiredAssets() => Editing.Values.GetRequiredAssetsFromIBoundsProvider();

    public bool GetTranslations(ref TranslationStore translations)
    {
        bool requiresLoad = false;
        foreach (FurnitureDelimString furniDelim in Editing.Values)
        {
            furniDelim.UpdateForFirstTimeSerialize();
            translations.Data[furniDelim.DisplayNameImpl.Key] = furniDelim.DisplayNameImpl.Value ?? "???";
            requiresLoad = requiresLoad || furniDelim.DisplayNameImpl.Kind == TranslationStringKind.LocalizedText;
        }
        return requiresLoad;
    }

    public void SetTranslations(TranslationStore? translations)
    {
        ModEntry.Log($"SetTranslations {translations}");
        if (translations == null)
            return;
        foreach (FurnitureDelimString furniDelim in Editing.Values)
        {
            furniDelim.DisplayNameImpl.SetValueFrom(translations);
        }
    }
}
