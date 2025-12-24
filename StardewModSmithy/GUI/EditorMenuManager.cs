using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModSmithy.GUI.EditorContext;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy.GUI;

internal static class EditorMenuManager
{
    private static IViewEngine viewEngine = null!;
    private const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    private const string VIEW_EDIT_FURNITURE = $"{VIEW_ASSET_PREFIX}/edit-furniture";
    private const string VIEW_EDIT_TEXTURE_STORE = $"{VIEW_ASSET_PREFIX}/edit-texture-store";
    private static readonly PerScreen<DraggableTextureContext?> draggableTextureCtx = new();
    private static IModHelper helper = null!;

    private static readonly KeybindList toggleMovingMode = new(SButton.MouseMiddle);

    internal static void Register(IModHelper helper)
    {
        EditorMenuManager.helper = helper;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    internal static void ShowFurnitureEditor(TextureAssetGroup textureAssetGroup, FurnitureAsset furnitureAsset)
    {
        DraggableTextureContext draggableTextureContext = new(textureAssetGroup);
        FurnitureAssetContext furnitureAssetContext = new(furnitureAsset);
        BaseEditorContext ctx = new(draggableTextureContext, furnitureAssetContext);
        if (furnitureAssetContext.FurnitureDataList.Count > 0)
        {
            furnitureAssetContext.BoundsProvider = furnitureAssetContext.FurnitureDataList[0];
        }
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_EDIT_FURNITURE, ctx);
        draggableTextureCtx.Value = ctx.TextureContext;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged_FurniEdit;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is null)
        {
            if (draggableTextureCtx.Value is not null)
            {
                draggableTextureCtx.Value = null;
                helper.Events.Input.ButtonsChanged -= OnButtonsChanged_FurniEdit;
            }
        }
    }

    private static void OnButtonsChanged_FurniEdit(object? sender, ButtonsChangedEventArgs e)
    {
        if (toggleMovingMode.JustPressed())
        {
            draggableTextureCtx.Value?.ToggleMovementMode();
        }
    }
}
