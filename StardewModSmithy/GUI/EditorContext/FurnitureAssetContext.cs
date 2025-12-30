using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

public partial class FurnitureAssetContext(FurnitureAsset furnitureAsset) : AbstractEditableAssetContext()
{
    private readonly FurnitureAsset furnitureAsset = furnitureAsset;
    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) =>
        delimStr.FromDeserialize ? delimStr.DisplayName : I18n.Gui_Placeholder(delimStr.PreSerializeSeq);

    [DependsOn(nameof(BoundsProvider))]
    public FurnitureDelimString? SelectedFurniture => (FurnitureDelimString?)BoundsProvider;

    [Notify]
    public bool textureHasAtlas = false;

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

    private void OnSelectedFurniturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FurnitureDelimString.DisplayName))
        {
            OnPropertyChanged(new(nameof(FurnitureDataList)));
        }
    }

    public void Create()
    {
        FurnitureDelimString furni = furnitureAsset.AddNewDefault();
        furni.TextureAssetName = SelectedTextureAsset.AssetName;
        OnPropertyChanged(new(nameof(FurnitureDataList)));
        BoundsProvider = furni;
    }

    public void Delete()
    {
        if (furnitureAsset.Delete(SelectedFurniture))
        {
            OnPropertyChanged(new(nameof(FurnitureDataList)));
            if (FurnitureDataList.Count > 0)
                BoundsProvider = FurnitureDataList[FurnitureDataList.Count - 1];
            else
                BoundsProvider = null;
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
            furni.TextureAssetName = SelectedTextureAsset.AssetName;
        }
        OnPropertyChanged(new(nameof(FurnitureDataList)));
        BoundsProvider = FurnitureDataList[0];
    }
}
