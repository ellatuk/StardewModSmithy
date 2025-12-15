using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy.GUI;

internal static class EditorMenuManager
{
    private static IViewEngine viewEngine = null!;
    private const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    private const string VIEW_FURNITURE_EDITOR = $"{VIEW_ASSET_PREFIX}/furniture-editor";
    private static readonly PerScreen<FurnitureEditorContext?> furniEditCtx = new();
    private static IModHelper helper = null!;

    private static KeybindList toggleMovingMode = new(SButton.MouseMiddle);

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

    internal static void ShowFurnitureEditor(TextureAsset textureAsset, FurnitureAsset furnitureAsset)
    {
        FurnitureEditorContext ctx = new(textureAsset, furnitureAsset);
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_FURNITURE_EDITOR, ctx);
        furniEditCtx.Value = ctx;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged_FurniEdit;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is null)
        {
            if (furniEditCtx.Value is not null)
            {
                furniEditCtx.Value = null;
                helper.Events.Input.ButtonsChanged -= OnButtonsChanged_FurniEdit;
            }
        }
    }

    private static void OnButtonsChanged_FurniEdit(object? sender, ButtonsChangedEventArgs e)
    {
        if (toggleMovingMode.JustPressed())
        {
            furniEditCtx.Value?.ToggleMovementMode();
        }
    }
}
