using PropertyChanged.SourceGenerator;
using StardewModSmithy.Models;

namespace StardewModSmithy.GUI.EditorContext;

public partial class FurnitureAssetContext(FurnitureAsset furnitureAsset) : AbstractEditableAssetContext()
{
    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) =>
        delimStr.FromDeserialize ? delimStr.DisplayName : string.Concat("NEW#", delimStr.PreSerializeSeq.ToString());

    [DependsOn(nameof(BoundsProvider))]
    public FurnitureDelimString? SelectedFurniture => (FurnitureDelimString?)BoundsProvider;

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
    }

    public void Create()
    {
        FurnitureDelimString furni = furnitureAsset.AddNewDefault(SelectedFurniture);
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
}
