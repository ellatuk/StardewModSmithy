using StardewModSmithy.Models;

namespace StardewModSmithy.GUI.EditorContext;

public class FurnitureAssetContext(FurnitureAsset furnitureAsset) : AbstractEditableAssetContext()
{
    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) => delimStr.Name;

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
}
