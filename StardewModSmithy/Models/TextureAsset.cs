using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewValley;

namespace StardewModSmithy.Models;

public sealed partial record TextureAsset(IAssetName AssetName, string PathOnDisk)
{
    private Texture2D? texture = null;
    public Texture2D Texture
    {
        get
        {
            texture ??= ModEntry.ModContent.Load<Texture2D>(PathOnDisk);
            return texture;
        }
    }

    public void Reload() => texture = null;

    public SDUISprite GetUISprite(float scale) =>
        new(Texture, SourceRect: Texture.Bounds, FixedEdges: new(0), SliceSettings: new(Scale: scale))
        {
            AssetName = AssetName,
        };

    public SDUISprite UISprite => GetUISprite(4);
    public SDUISprite UISpriteSmall => GetUISprite(1);

    /// <summary>
    /// Inset-style background and border, often used to hold an item or represent a slot.
    /// </summary>
    public static SDUISprite MenuSlotInset =>
        new(Game1.menuTexture, SourceRect: new(0, 320, 60, 60), FixedEdges: new(4, 10, 12, 4), new(Scale: 1));

    public static SDUISprite MenuSlotTransparent =>
        new(Game1.menuTexture, SourceRect: new(128, 128, 64, 64), FixedEdges: new(4), new(Scale: 1));

    [Notify]
    public bool isSelected = false;
}

public sealed class TextureAssetGroup(string group, Dictionary<IAssetName, TextureAsset> gatheredTextures)
    : ILoadableAsset
{
    public string Group { get; set; } = group;

    public Dictionary<IAssetName, TextureAsset> GatheredTextures { get; set; } = gatheredTextures;

    public string Target => string.Join(',', GatheredTextures.Keys.Select(target => target.BaseName));
    public string FromFile => Path.Join(Group, "{{ModId}}", "{{TargetWithoutPath}}");

    public static IAssetName FormAssetNameForGroup(string group, string fileName) =>
        ModEntry.ParseAssetName(Path.Join(group, "{{ModId}}", Path.GetFileNameWithoutExtension(fileName)));

    public static TextureAssetGroup FromSourceDir(string sourceDir, string group)
    {
        Dictionary<IAssetName, TextureAsset> gatheredTextures = [];
        foreach (string file in Directory.GetFiles(Path.Combine(ModEntry.DirectoryPath, sourceDir)))
        {
            if (!file.EndsWith(".png"))
                continue;
            string relFile = Path.GetRelativePath(ModEntry.DirectoryPath, file);
            IAssetName assetName = FormAssetNameForGroup(group, file);
            gatheredTextures[assetName] = new(assetName, relFile);
        }
        return new TextureAssetGroup(group, gatheredTextures);
    }

    public void StageFiles(string targetPath)
    {
        string targetGroupDir = Path.Combine(targetPath, Group);
        Directory.CreateDirectory(targetGroupDir);
        foreach ((IAssetName assetName, TextureAsset asset) in GatheredTextures)
        {
            File.Copy(
                Path.Combine(ModEntry.DirectoryPath, asset.PathOnDisk),
                Path.Combine(targetGroupDir, Path.GetFileName(assetName.BaseName) + ".png")
            );
        }
    }
}
