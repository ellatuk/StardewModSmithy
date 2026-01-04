using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;

namespace StardewModSmithy.GUI.EditorContext;

public partial class WallpaperFlooringAssetContext : AbstractEditableAssetContext
{
    private readonly WallpaperFlooringAsset wallpaperFlooringAsset;

    public List<EditableWallpaperOrFlooring> WallpaperFlooringDataList;

    public readonly IBoundsProviderSpinBoxViewModel BoundsProviderSelector;

    public WallpaperFlooringAssetContext(WallpaperFlooringAsset wallpaperFlooringAsset)
        : base()
    {
        this.wallpaperFlooringAsset = wallpaperFlooringAsset;
        this.WallpaperFlooringDataList = wallpaperFlooringAsset.Editing.Values.ToList();
        if (this.WallpaperFlooringDataList.Count != 0)
        {
            BoundsProvider = this.WallpaperFlooringDataList[0];
        }
        this.BoundsProviderSelector = new(() => BoundsProvider, (value) => BoundsProvider = value)
        {
            BoundsProviderList = this.WallpaperFlooringDataList,
        };
    }
}
