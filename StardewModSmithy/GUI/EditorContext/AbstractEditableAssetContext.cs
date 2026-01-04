using System.Globalization;
using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.GUI.EditorContext;

public abstract partial class AbstractEditableAssetContext
{
    internal event EventHandler<IBoundsProvider?>? BoundsProviderChanged;

    public AbstractEditableAssetContext()
    {
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == "BoundsProvider")
            {
                BoundsProviderChanged?.Invoke(this, BoundsProvider);
            }
        };
    }

    [Notify]
    private IBoundsProvider? boundsProvider;

    [Notify]
    private string lastSavedMessage = string.Empty;

    internal Action? saveChangesDelegate;
    internal TextureAsset SelectedTextureAsset = null!;

    public bool HasBoundsProvider => BoundsProvider != null;

    public virtual void SetSpriteIndex(object? sender, int spriteIndex) { }

    public virtual void SetTexture(object? sender, TextureAsset textureAsset)
    {
        SelectedTextureAsset = textureAsset;
    }

    public void SaveChanges(AutosaveFrequencyMode saveReason)
    {
        if (ModEntry.Config.AutosaveFrequency == saveReason)
        {
            Save();
        }
    }

    public void Save()
    {
        string now = DateTime.Now.ToString(Game1.content.CurrentCulture);
        LastSavedMessage = I18n.Message_LastSavedAt(time: now);
        saveChangesDelegate?.Invoke();
    }
}
