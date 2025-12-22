using StardewModdingAPI;

namespace StardewModSmithy.Models.Interfaces;

public interface ILoadableAsset
{
    public ValueTuple<string, string>? StageAndGetTargetAndFromFile(
        string targetPath,
        ref HashSet<IAssetName> requiredAssets
    );
}
