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
        this.BoundsProviderSelector.Value = BoundsProvider;
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
        UpdateFurnitureDataList();
        TextureHasAtlas = textureAsset?.TextureAtlas != null;
    }

    public void Create()
    {
        FurnitureDelimString furni = furnitureAsset.AddNewDefault();
        furni.TextureAssetName = SelectedTextureAsset.AssetName;
        UpdateFurnitureDataList();
        BoundsProvider = furni;
    }

    public void Delete()
    {
        if (furnitureAsset.Delete(SelectedFurniture))
        {
            if (FurnitureDataList.Count > 0)
                BoundsProvider = FurnitureDataList[FurnitureDataList.Count - 1];
            else
                BoundsProvider = null;
            UpdateFurnitureDataList();
        }
    }

    public void PopulateFromAtlas()
    {
        if (SelectedTextureAsset.TextureAtlas == null)
            return;
        int indexColCnt = SelectedTextureAsset.Texture.Bounds.Width / Consts.TX_TILE;
        foreach (TxAtlasEntry entry in SelectedTextureAsset.TextureAtlas)
        {
            FurnitureDelimString furni = furnitureAsset.AddNewDefault();
            furni.DisplayName = Path.GetFileNameWithoutExtension(entry.RelPath);
            furni.SpriteIndex = entry.Area.Y / Consts.TX_TILE * indexColCnt + entry.Area.X / Consts.TX_TILE;
            furni.TilesheetSize = new(entry.Area.Width / Consts.TX_TILE, entry.Area.Height / Consts.TX_TILE);
            furni.BoundingBoxSize = new(furni.TilesheetSize.X, 1);
            furni.TextureAssetName = SelectedTextureAsset.AssetName;
            furni.UpdateForFirstTimeSerialize();
        }
        UpdateFurnitureDataList();
        BoundsProvider = FurnitureDataList[0];
    }
}
