using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModSmithy.GUI.EditorContext;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;
using StardewValley.Menus;

namespace StardewModSmithy.GUI;

internal static class EditorMenuManager
{
    private static IViewEngine viewEngine = null!;
    private const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    private const string VIEW_WORKSPACE = $"{VIEW_ASSET_PREFIX}/workspace";
    private const string VIEW_EDIT_FURNITURE = $"{VIEW_ASSET_PREFIX}/edit-furniture";
    private const string VIEW_EDIT_WALLFLOOR = $"{VIEW_ASSET_PREFIX}/edit-wallfloor";
    private static readonly PerScreen<PackListingContext?> listingContext = new();
    private static readonly PerScreen<BaseEditorContext?> editorContext = new();
    internal static readonly PerScreen<bool> showWorkspaceNextTick = new();
    private static IModHelper helper = null!;

    private static readonly KeybindList toggleMovingMode = new(SButton.MouseMiddle);

    internal static void Register(IModHelper helper)
    {
        EditorMenuManager.helper = helper;
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
    }

    private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (Context.IsWorldReady)
        {
            if (Game1.activeClickableMenu != null)
                return;
        }
        else
        {
            if (TitleMenu.subMenu != null)
                return;
        }

        if (showWorkspaceNextTick.Value)
        {
            showWorkspaceNextTick.Value = false;
            ShowWorkspace();
        }
    }

    private static void OnButtonsChanged_DragSheet(object? sender, ButtonsChangedEventArgs e)
    {
        if (toggleMovingMode.JustPressed())
        {
            editorContext.Value?.TextureContext.ToggleMovementMode();
        }
    }

    internal static void ShowWorkspace()
    {
        if (Context.IsWorldReady)
            Game1.exitActiveMenu();

        BaseWorkspaceContext ctx = new(listingContext.Value ??= PackListingContext.Initialize(), new(ModEntry.Config));

        IClickableMenu menu = viewEngine.CreateMenuFromAsset(VIEW_WORKSPACE, ctx);
        if (Context.IsWorldReady)
        {
            Game1.activeClickableMenu = menu;
        }
        else
        {
            TitleMenu.subMenu = menu;
        }
    }

    private static void ShowEditor(
        Action? saveChanges,
        DraggableTextureContext draggableTextureContext,
        AbstractEditableAssetContext editableContext,
        string editorViewName
    )
    {
        if (Context.IsWorldReady)
            Game1.exitActiveMenu();

        BaseEditorContext ctx = new(draggableTextureContext, editableContext, saveChanges);

        IMenuController ctrl = viewEngine.CreateMenuControllerFromAsset(editorViewName, ctx);

        ctrl.Closing += EditorClosing;
        if (draggableTextureContext.CanDrag)
        {
            editorContext.Value = ctx;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged_DragSheet;
        }

        Game1.activeClickableMenu = ctrl.Menu;
    }

    internal static void EditorClosing()
    {
        if (editorContext.Value is not null)
        {
            editorContext.Value.EditableContext.AutoSaveChanges(Wheels.AutosaveFrequencyMode.OnExit);
            editorContext.Value = null;
            helper.Events.Input.ButtonsChanged -= OnButtonsChanged_DragSheet;
        }
        showWorkspaceNextTick.Value = true;
    }

    #region furniture
    internal static void ShowFurnitureEditor(
        TextureAssetGroup textureAssetGroup,
        FurnitureAsset furnitureAsset,
        Action? saveChanges
    )
    {
        if (
            DraggableTextureContext.Initialize(textureAssetGroup, enableFront: true, dragAllow: DragAllowMode.Allowed)
            is not DraggableTextureContext draggableTextureContext
        )
            return;
        FurnitureAssetContext furnitureAssetContext = new(furnitureAsset);
        ShowEditor(saveChanges, draggableTextureContext, furnitureAssetContext, VIEW_EDIT_FURNITURE);
    }
    #endregion

    #region wallpaper and flooring
    internal static void ShowWallpaperAndFlooringEditor(
        TextureAssetGroup textureAssetGroup,
        WallpaperFlooringAsset wallpaperFlooringAsset,
        Action? saveChanges
    )
    {
        if (
            DraggableTextureContext.Initialize(
                textureAssetGroup,
                textureFilter: WallpaperFlooringAsset.TextureFilter,
                dragAllow: DragAllowMode.SheetOnlyUncapped
            )
            is not DraggableTextureContext draggableTextureContext
        )
            return;
        WallpaperFlooringAssetContext wallpaperFlooringAssetContext = new(wallpaperFlooringAsset);
        ShowEditor(saveChanges, draggableTextureContext, wallpaperFlooringAssetContext, VIEW_EDIT_WALLFLOOR);
    }
    #endregion
}
