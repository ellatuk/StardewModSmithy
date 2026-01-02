using PropertyChanged.SourceGenerator;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

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
            saveChangesDelegate?.Invoke();
    }

    public void Save()
    {
        saveChangesDelegate?.Invoke();
    }
}
