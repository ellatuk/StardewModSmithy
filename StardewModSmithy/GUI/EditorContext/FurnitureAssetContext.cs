using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

public partial class FurnitureAssetContext : AbstractEditableAssetContext
{
    private readonly FurnitureAsset furnitureAsset;

    public List<FurnitureDelimString> FurnitureDataList;

    public readonly IBoundsProviderSpinBoxViewModel BoundsProviderSelector;

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) =>
        delimStr.FromDeserialize ? delimStr.DisplayName : I18n.Gui_Placeholder(delimStr.PreSerializeSeq);

    [DependsOn(nameof(BoundsProvider))]
    public FurnitureDelimString? SelectedFurniture => (FurnitureDelimString?)BoundsProvider;

    [Notify]
    public bool textureHasAtlas = false;

    public FurnitureAssetContext(FurnitureAsset furnitureAsset)
        : base()
    {
        this.furnitureAsset = furnitureAsset;
        this.FurnitureDataList = furnitureAsset.Editing.Values.ToList();
        if (this.FurnitureDataList.Count != 0)
        {
            BoundsProvider = this.FurnitureDataList[0];
        }
        this.BoundsProviderSelector = new(() => BoundsProvider, (value) => BoundsProvider = value)
        {
            furnitureDataList = this.FurnitureDataList,
        };
    }

    private void UpdateFurnitureDataList()
    {
        this.FurnitureDataList = furnitureAsset.Editing.Values.ToList();
        this.BoundsProviderSelector.furnitureDataList = this.FurnitureDataList;
    }

    public override void SetSpriteIndex(object? sender, int spriteIndex)
    {
        base.SetSpriteIndex(sender, spriteIndex);
        if (SelectedFurniture != null && spriteIndex >= 0)
        {
            SelectedFurniture.SpriteIndex = spriteIndex;
        }
    }

    public override void SetTexture(object? sender, TextureAsset textureAsset)
    {
        base.SetTexture(sender, textureAsset);
        SelectedFurniture?.TextureAssetName = textureAsset.AssetName;
        TextureHasAtlas = textureAsset?.TextureAtlas != null;
    }

    public void Create()
    {
        FurnitureDelimString furni = furnitureAsset.AddNewDefault();
        furni.TextureAssetName = SelectedTextureAsset.AssetName;
        UpdateFurnitureDataList();
        this.BoundsProviderSelector.Value = furni;
        this.BoundsProviderSelector.SeekIndex();
        SaveChanges(AutosaveFrequencyMode.OnAddOrRemove);
    }

    public void Delete()
    {
        if (SelectedFurniture == null)
            return;
        if (furnitureAsset.Delete(SelectedFurniture))
        {
            UpdateFurnitureDataList();
            this.BoundsProviderSelector.ClampIndex();
            BoundsProvider = this.BoundsProviderSelector.Value;
            SaveChanges(AutosaveFrequencyMode.OnAddOrRemove);
        }
    }

    public void PopulateFromAtlas()
    {
        if (SelectedTextureAsset.TextureAtlas == null)
            return;
        int indexColCnt = SelectedTextureAsset.Texture.Bounds.Width / Consts.TX_TILE;
        FurnitureDelimString? firstFurni = null;
        foreach (TxAtlasEntry entry in SelectedTextureAsset.TextureAtlas)
        {
            FurnitureDelimString furni = furnitureAsset.AddNewDefault();
            furni.DisplayName = Path.GetFileNameWithoutExtension(entry.RelPath);
            furni.SpriteIndex = entry.Area.Y / Consts.TX_TILE * indexColCnt + entry.Area.X / Consts.TX_TILE;
            furni.TilesheetSize = new(entry.Area.Width / Consts.TX_TILE, entry.Area.Height / Consts.TX_TILE);
            furni.BoundingBoxSize = new(furni.TilesheetSize.X, 1);
            furni.TextureAssetName = SelectedTextureAsset.AssetName;
            furni.UpdateForFirstTimeSerialize();
            firstFurni ??= furni;
        }
        UpdateFurnitureDataList();
        this.BoundsProviderSelector.Value = firstFurni;
        this.BoundsProviderSelector.SeekIndex();
        SaveChanges(AutosaveFrequencyMode.OnAddOrRemove);
    }
}
