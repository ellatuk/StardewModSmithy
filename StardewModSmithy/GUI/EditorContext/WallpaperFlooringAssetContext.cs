using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

public partial class WallpaperFlooringAssetContext : AbstractEditableAssetContext
{
    private readonly WallpaperFlooringAsset wallpaperFlooringAsset;

    public List<EditableWallpaperOrFlooring> WallpaperFlooringDataList;

    [DependsOn(nameof(BoundsProvider))]
    public EditableWallpaperOrFlooring? Selected => (EditableWallpaperOrFlooring?)BoundsProvider;

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

    private void UpdateDataList()
    {
        this.WallpaperFlooringDataList = wallpaperFlooringAsset.Editing.Values.ToList();
        this.BoundsProviderSelector.BoundsProviderList = this.WallpaperFlooringDataList;
    }

    public override void SetTexture(object? sender, TextureAsset textureAsset)
    {
        base.SetTexture(sender, textureAsset);
        Selected?.TextureAssetName = textureAsset.AssetName;
    }

    public override void Create()
    {
        EditableWallpaperOrFlooring wallfloor = wallpaperFlooringAsset.AddNewDefault(SelectedTextureAsset.AssetName);
        UpdateDataList();
        this.BoundsProviderSelector.Value = wallfloor;
        this.BoundsProviderSelector.SeekIndex();
        AutoSaveChanges(AutosaveFrequencyMode.OnAdd);
    }

    public override void Delete()
    {
        if (Selected == null)
            return;
        if (wallpaperFlooringAsset.Delete(Selected))
        {
            UpdateDataList();
            this.BoundsProviderSelector.ClampIndex();
            BoundsProvider = this.BoundsProviderSelector.Value;
        }
    }
}
