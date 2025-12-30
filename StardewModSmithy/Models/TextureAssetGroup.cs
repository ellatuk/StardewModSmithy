using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models;

public record TxAtlasEntry(string RelPath, Rectangle Area);

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

    public List<TxAtlasEntry>? TextureAtlas = TryGetTextureAtlas(PathOnDisk);

    private static List<TxAtlasEntry>? TryGetTextureAtlas(string PathOnDisk)
    {
        string atlasPath = Path.Combine(
            ModEntry.DirectoryPath,
            Path.GetDirectoryName(PathOnDisk) ?? "",
            string.Concat(Path.GetFileNameWithoutExtension(PathOnDisk), Consts.ATLAS_SUFFIX)
        );
        ModEntry.Log(atlasPath);
        if (!File.Exists(atlasPath))
            return null;
        return ModEntry.ReadJson<List<TxAtlasEntry>>(atlasPath);
    }

    public void Reload() => texture = null;

    public SDUISprite GetUISprite(float scale) =>
        new(Texture, SourceRect: Texture.Bounds, FixedEdges: new(0), SliceSettings: new(Scale: scale))
        {
            AssetName = AssetName,
        };

    public TextureAsset? Front = null;

    public SDUISprite UISprite => GetUISprite(4);
    public SDUISprite UISpriteSmall => GetUISprite(1);

    [Notify]
    public bool isSelected = false;

    [Notify]
    public bool isSelectedFront = false;
}

public sealed class TextureAssetGroup(string group, Dictionary<IAssetName, TextureAsset> gatheredTextures)
    : ILoadableAsset
{
    public string Group { get; set; } = group;

    public bool EnableFront { get; set; } = true;

    public Dictionary<IAssetName, TextureAsset> GatheredTextures { get; set; } = gatheredTextures;

    public ValueTuple<string, string>? StageAndGetTargetAndFromFile(
        string targetPath,
        ref HashSet<IAssetName> requiredAssets
    )
    {
        string targetGroupDir = Path.Combine(targetPath, Group);
        Directory.CreateDirectory(targetGroupDir);
        StringBuilder targetSB = new();
        foreach (IAssetName key in requiredAssets.Reverse())
        {
            if (!GatheredTextures.TryGetValue(key, out TextureAsset? txAsset))
            {
                continue;
            }
            File.Copy(
                Path.Combine(ModEntry.DirectoryPath, txAsset.PathOnDisk),
                Path.Combine(targetGroupDir, Path.GetFileName(key.BaseName) + ".png")
            );
            targetSB.Append(',');
            targetSB.Append(key.BaseName);
            requiredAssets.Remove(key);
            if (txAsset.Front is TextureAsset txAssetFront)
            {
                File.Copy(
                    Path.Combine(ModEntry.DirectoryPath, txAssetFront.PathOnDisk),
                    Path.Combine(targetGroupDir, Path.GetFileName(key.BaseName) + "Front.png")
                );
                targetSB.Append(',');
                targetSB.Append(key.BaseName);
                targetSB.Append("Front");
            }
        }
        if (targetSB.Length > 1)
        {
            targetSB.Remove(0, 1);
            return new(targetSB.ToString(), Path.Join("", Group, "{{TargetWithoutPath}}.png"));
        }
        return null;
    }

    public static IAssetName FormAssetNameForGroup(string group, string fileName) =>
        ModEntry.ParseAssetName(Path.Join(group, "{{ModId}}", Path.GetFileNameWithoutExtension(fileName)));

    public static TextureAssetGroup FromSourceDir(string group)
    {
        Dictionary<IAssetName, TextureAsset> gatheredTextures = [];
        string fullSourceDir = Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT);
        foreach (string dir in Directory.GetDirectories(fullSourceDir))
        {
            SpritePacker.Pack(dir);
        }
        foreach (string file in Directory.GetFiles(fullSourceDir))
        {
            if (!file.EndsWith(".png"))
                continue;
            string relFile = Path.GetRelativePath(ModEntry.DirectoryPath, file);
            IAssetName assetName = FormAssetNameForGroup(group, file);
            gatheredTextures[assetName] = new(assetName, relFile);
        }
        if (gatheredTextures.Any())
        {
            gatheredTextures.First().Value.IsSelected = true;
        }
        return new TextureAssetGroup(group, gatheredTextures);
    }
}
