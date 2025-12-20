using StardewModSmithy.Models;

namespace StardewModSmithy.GUI.EditorContext;

internal sealed class TextureStoreContext(TextureAssetGroup textureAssetGroup)
{
    public IEnumerable<TextureAsset> Textures => textureAssetGroup.GatheredTextures.Values;

    public void SelectTextureAsset(TextureAsset selectedAsset)
    {
        foreach (TextureAsset asset in textureAssetGroup.GatheredTextures.Values)
        {
            asset.IsSelected = asset == selectedAsset;
        }
    }
}
