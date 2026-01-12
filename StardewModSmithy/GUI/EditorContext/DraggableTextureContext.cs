using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

public enum DragMovementMode
{
    Sheet = 0,
    Bounds = 1,
}

public enum DragAllowMode
{
    None = 0,
    SheetOnlyUncapped = 1,
    Allowed = 2,
}

public partial class DraggableTextureContext(
    TextureAssetGroup textureAssetGroup,
    Func<TextureAsset, bool>? textureFilter,
    DragAllowMode dragAllow,
    bool enableFront,
    int tileUnit
)
{
    public static DraggableTextureContext? Initialize(
        TextureAssetGroup textureAssetGroup,
        Func<TextureAsset, bool>? textureFilter = null,
        DragAllowMode dragAllow = DragAllowMode.Allowed,
        bool enableFront = false,
        int tileUnit = Utils.DRAW_TILE
    )
    {
        if (textureFilter != null && !textureAssetGroup.GatheredTextures.Values.Any(textureFilter))
        {
            return null;
        }
        return new DraggableTextureContext(textureAssetGroup, textureFilter, dragAllow, enableFront, tileUnit);
    }

    public event EventHandler<int>? Dragged;
    public event EventHandler<TextureAsset>? TextureChanged;

    public bool CanDrag => tileUnit > 0 && dragAllow != DragAllowMode.None;
    public bool CanChangeMode => tileUnit > 0 && dragAllow == DragAllowMode.Allowed;
    public bool AlwaysSyncDrag => dragAllow == DragAllowMode.SheetOnlyUncapped;

    [Notify]
    public TextureAsset selected =
        textureFilter == null
            ? textureAssetGroup.GatheredTextures.Values.First()
            : textureAssetGroup.GatheredTextures.Values.First(textureFilter);

    public TextureAsset? SelectedFront
    {
        get => Selected.Front;
        set
        {
            Selected.Front = value;
            foreach (TextureAsset asset in textureAssetGroup.GatheredTextures.Values)
            {
                asset.IsSelectedFront = asset == Selected.Front;
            }
            OnPropertyChanged(new(nameof(SelectedFront)));
        }
    }

    public SDUISprite Sheet => Selected.UISprite;

    [DependsOn(nameof(SelectedFront))]
    public bool HasSheetFront => SelectedFront != null;

    [DependsOn(nameof(SelectedFront))]
    public SDUISprite? SheetFront => SelectedFront?.UISprite;

    public EnumSegmentsViewModel<DragMovementMode> DragMode = new()
    {
        SelectedValue = dragAllow == DragAllowMode.SheetOnlyUncapped ? DragMovementMode.Sheet : DragMovementMode.Bounds,
    };

    [Notify]
    public SDUIEdges sheetMargin = new(0, 0, 0, 0);

    [Notify]
    public float sheetOpacity = 1f;

    public float SheetOpacityFront => SheetOpacity * 0.5f;

    [Notify]
    public SDUIEdges boundsPadding =
        dragAllow == DragAllowMode.Allowed ? new(2 * tileUnit, 2 * tileUnit, 0, 0) : new(0);

    [Notify]
    private IBoundsProvider? boundsProvider = null;

    public bool HasBoundsProvider => BoundsProvider != null;

    [Notify]
    public int spriteIndex = 0;

    [Notify]
    private bool showingTextureSelector = false;

    public IEnumerable<TextureAsset> Textures =>
        textureFilter == null
            ? textureAssetGroup.GatheredTextures.Values
            : textureAssetGroup.GatheredTextures.Values.Where(textureFilter);

    public void SelectTextureAsset(TextureAsset selectedAsset)
    {
        foreach (TextureAsset asset in textureAssetGroup.GatheredTextures.Values)
        {
            asset.IsSelected = asset == selectedAsset;
            asset.IsSelectedFront = asset == selectedAsset.Front;
        }
        selectedAsset.IsSelectedFront = false;
        Selected = selectedAsset;
        TextureChanged?.Invoke(this, selectedAsset);
    }

    public void SelectTextureAssetFront(TextureAsset selectedAsset)
    {
        if (!enableFront)
            return;
        if (selectedAsset.IsSelected)
            return;
        if (selectedAsset == SelectedFront)
        {
            SelectedFront = null;
        }
        else
        {
            SelectedFront = selectedAsset;
        }
    }

    public void UpdateSpriteIndex(SDUIEdges newSheetMargin, SDUIEdges newBoundsPadding)
    {
        if (!CanDrag)
            return;

        if (boundsProvider == null)
        {
            if (DragMode.SelectedValue == DragMovementMode.Bounds)
                BoundsPadding = newBoundsPadding;
            else
                SheetMargin = newSheetMargin;
            return;
        }
        int xDelta = (newBoundsPadding.Left - newSheetMargin.Left) / tileUnit;
        int yDelta = (newBoundsPadding.Top - newSheetMargin.Top) / tileUnit;

        xDelta = Math.Max(0, Math.Min(xDelta, Sheet.IndexColCnt - boundsProvider.TilesheetSize.X));
        yDelta = Math.Max(0, Math.Min(yDelta, Sheet.IndexRowCnt - boundsProvider.TilesheetSize.Y));
        int oldSpriteIndex = SpriteIndex;
        SpriteIndex = yDelta * Sheet.IndexColCnt + xDelta;
        if (oldSpriteIndex != SpriteIndex)
        {
            Dragged?.Invoke(this, SpriteIndex);
        }

        if (DragMode.SelectedValue == DragMovementMode.Bounds)
        {
            BoundsPadding = new(newSheetMargin.Left + xDelta * tileUnit, newSheetMargin.Top + yDelta * tileUnit, 0, 0);
        }
        else
        {
            SheetMargin = new(
                newBoundsPadding.Left - xDelta * tileUnit,
                newBoundsPadding.Top - yDelta * tileUnit,
                0,
                0
            );
        }
    }

    public void SetSpriteIndexForBoundsProvider(IBoundsProvider? boundsProvider)
    {
        if (boundsProvider == null)
            return;
        SpriteIndex = boundsProvider.SpriteIndex;
        int xPos = SpriteIndex % Sheet.IndexColCnt;
        int yPos = SpriteIndex / Sheet.IndexColCnt;

        SheetMargin = new(BoundsPadding.Left - xPos * tileUnit, BoundsPadding.Top - yPos * tileUnit, 0, 0);
    }

    private Vector2 lastDragPos = new(-1, -1);

    public void SheetDragStart(Vector2 position)
    {
        if (!CanDrag)
            return;

        lastDragPos = position;
        SheetOpacity = 0.7f;
    }

    public void SheetDrag(Vector2 position)
    {
        if (!CanDrag)
            return;

        int newOffsetX = 0;
        int newOffsetY = 0;

        Vector2 dragChange = position - lastDragPos;
        int dragTileCnt;
        bool changed = false;
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.X) / tileUnit) * tileUnit) > 0)
        {
            dragTileCnt *= Math.Sign(dragChange.X);
            newOffsetX += dragTileCnt;
            lastDragPos.X += dragTileCnt;
            changed = true;
        }
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.Y) / tileUnit) * tileUnit) > 0)
        {
            dragTileCnt *= Math.Sign(dragChange.Y);
            newOffsetY += dragTileCnt;
            lastDragPos.Y += dragTileCnt;
            changed = true;
        }

        if (AlwaysSyncDrag || ModEntry.Config.SyncDragKey.IsDown())
        {
            SheetMargin = new(sheetMargin.Left + newOffsetX, sheetMargin.Top + newOffsetY, 0, 0);
            BoundsPadding = new(boundsPadding.Left + newOffsetX, boundsPadding.Top + newOffsetY, 0, 0);
            return;
        }

        if (DragMode.SelectedValue == DragMovementMode.Bounds)
        {
            newOffsetX += boundsPadding.Left;
            newOffsetY += boundsPadding.Top;
        }
        else
        {
            newOffsetX += sheetMargin.Left;
            newOffsetY += sheetMargin.Top;
        }

        if (changed)
        {
            if (DragMode.SelectedValue == DragMovementMode.Bounds)
            {
                UpdateSpriteIndex(sheetMargin, new(newOffsetX, newOffsetY, 0, 0));
            }
            else
            {
                UpdateSpriteIndex(new(newOffsetX, newOffsetY, 0, 0), boundsPadding);
            }
        }
    }

    public void SheetDragEnd(Vector2 position)
    {
        if (!CanDrag)
            return;

        lastDragPos = new(-1, -1);
        SheetOpacity = 1f;
        if (SpriteIndex < 0)
        {
            SpriteIndex = 0;
            BoundsPadding = new(SheetMargin.Left, SheetMargin.Top, 0, 0);
        }
        Dragged?.Invoke(this, SpriteIndex);
    }

    public void ToggleDragMode()
    {
        if (AlwaysSyncDrag)
            return;
        DragMode.SelectedValue =
            DragMode.SelectedValue == DragMovementMode.Sheet ? DragMovementMode.Bounds : DragMovementMode.Sheet;
    }

    public void OnEditorBoundsProviderChanged(object? sender, IBoundsProvider? e)
    {
        BoundsProvider = e;
        if (e == null)
        {
            ShowingTextureSelector = true;
            return;
        }
        if (
            e.TextureAssetName != null
            && textureAssetGroup.GatheredTextures.TryGetValue(e.TextureAssetName, out TextureAsset? desiredAsset)
        )
        {
            SelectTextureAsset(desiredAsset);
        }
        else
        {
            e.TextureAssetName = Selected.AssetName;
        }
        SetSpriteIndexForBoundsProvider(e);
    }

    public void ToggleTextureSelector()
    {
        ShowingTextureSelector = !ShowingTextureSelector;
    }
}
