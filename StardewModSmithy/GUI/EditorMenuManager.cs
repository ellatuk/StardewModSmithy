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
    private const string VIEW_PACK_LISTING = $"{VIEW_ASSET_PREFIX}/pack-listing";
    private const string VIEW_EDIT_FURNITURE = $"{VIEW_ASSET_PREFIX}/edit-furniture";
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

    internal static void ShowPackListing()
    {
        if (PackListingContext.Initialize() is not PackListingContext ctx)
            return;
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_PACK_LISTING, ctx);
    }

    internal static void ShowFurnitureEditor(
        TextureAssetGroup textureAssetGroup,
        FurnitureAsset furnitureAsset,
        Action? saveChanges,
        bool asFollowingMenu
    )
    {
        DraggableTextureContext draggableTextureContext = new(textureAssetGroup);
        FurnitureAssetContext furnitureAssetContext = new(furnitureAsset);
        BaseEditorContext ctx = new(draggableTextureContext, furnitureAssetContext, saveChanges);
        if (furnitureAssetContext.FurnitureDataList.Count > 0)
        {
            furnitureAssetContext.BoundsProvider = furnitureAssetContext.FurnitureDataList[0];
        }
        IMenuController ctrl = viewEngine.CreateMenuControllerFromAsset(VIEW_EDIT_FURNITURE, ctx);
        editorContext.Value = ctx;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged_DragSheet;
        ctrl.Closing += CloseFurnitureEditor;
        if (asFollowingMenu && Game1.activeClickableMenu is IClickableMenu priorMenu)
        {
            Game1.nextClickableMenu.Add(priorMenu);
        }
        Game1.activeClickableMenu = ctrl.Menu;
    }

    private static void CloseFurnitureEditor()
    {
        if (editorContext.Value is not null)
        {
            editorContext.Value.SaveChanges();
            editorContext.Value = null;
            helper.Events.Input.ButtonsChanged -= OnButtonsChanged_DragSheet;
        }
    }

    private static void OnButtonsChanged_DragSheet(object? sender, ButtonsChangedEventArgs e)
    {
        if (toggleMovingMode.JustPressed())
        {
            editorContext.Value?.TextureContext.ToggleMovementMode();
        }
    }
}
