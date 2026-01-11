using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;

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
        this.BoundsProviderSelector = new(() => BoundsProvider, (value) => BoundsProvider = value, AutoSaveChanges)
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
        if (
            WallpaperFlooringDataList.FirstOrDefault(v => v.TextureAssetName == textureAsset.AssetName)
            is EditableWallpaperOrFlooring matching
        )
        {
            this.BoundsProviderSelector.Value = matching;
            this.BoundsProviderSelector.SeekIndex();
        }
        else
        {
            Create();
        }
    }

    public override void SetSpriteIndex(object? sender, int spriteIndex) { }

    public override void Create()
    {
        if (
            wallpaperFlooringAsset.AddNewDefault(SelectedTextureAsset.AssetName)
            is not EditableWallpaperOrFlooring wallfloor
        )
            return;
        UpdateDataList();
        this.BoundsProviderSelector.Value = wallfloor;
        this.BoundsProviderSelector.SeekIndex();
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
