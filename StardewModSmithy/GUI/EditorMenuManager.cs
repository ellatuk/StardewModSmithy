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
    private static readonly PerScreen<BaseEditorContext?> editorContext = new();
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
        if (PackListingContext.Initialize() is not PackListingContext packListing)
            return;
        BaseWorkspaceContext ctx = new(packListing, new(ModEntry.Config));
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_WORKSPACE, ctx);
    }

    private static void ShowEditor(
        Action? saveChanges,
        bool asFollowingMenu,
        DraggableTextureContext draggableTextureContext,
        AbstractEditableAssetContext editableContext,
        string editorViewName
    )
    {
        BaseEditorContext ctx = new(draggableTextureContext, editableContext, saveChanges);

        IClickableMenu menu;
        if (draggableTextureContext.CanDrag)
        {
            IMenuController ctrl = viewEngine.CreateMenuControllerFromAsset(editorViewName, ctx);
            editorContext.Value = ctx;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged_DragSheet;
            ctrl.Closing += CloseEditor;
            menu = ctrl.Menu;
        }
        else
        {
            menu = viewEngine.CreateMenuFromAsset(editorViewName, ctx);
        }

        if (asFollowingMenu && Game1.activeClickableMenu is IClickableMenu priorMenu)
        {
            Game1.nextClickableMenu.Add(priorMenu);
        }
        Game1.activeClickableMenu = menu;
    }

    private static void CloseEditor()
    {
        if (editorContext.Value is not null)
        {
            editorContext.Value.EditableContext.SaveChanges(Wheels.AutosaveFrequencyMode.OnExit);
            editorContext.Value = null;
            helper.Events.Input.ButtonsChanged -= OnButtonsChanged_DragSheet;
        }
    }

    #region furniture
    internal static void ShowFurnitureEditor(
        TextureAssetGroup textureAssetGroup,
        FurnitureAsset furnitureAsset,
        Action? saveChanges,
        bool asFollowingMenu
    )
    {
        DraggableTextureContext draggableTextureContext = new(textureAssetGroup);
        FurnitureAssetContext furnitureAssetContext = new(furnitureAsset);
        ShowEditor(saveChanges, asFollowingMenu, draggableTextureContext, furnitureAssetContext, VIEW_EDIT_FURNITURE);
    }
    #endregion

    #region wallpaper and flooring
    internal static void ShowWallpaperAndFlooring(
        TextureAssetGroup textureAssetGroup,
        WallpaperFlooringAsset wallpaperFlooringAsset,
        Action? saveChanges,
        bool asFollowingMenu
    )
    {
        DraggableTextureContext draggableTextureContext = new(textureAssetGroup, canDrag: false);
        WallpaperFlooringAssetContext wallpaperFlooringAssetContext = new(wallpaperFlooringAsset);
        ShowEditor(
            saveChanges,
            asFollowingMenu,
            draggableTextureContext,
            wallpaperFlooringAssetContext,
            VIEW_EDIT_WALLFLOOR
        );
    }
    #endregion
}
