namespace StardewModSmithy.GUI.EditorContext;

public sealed class BaseEditorContext
{
    public DraggableTextureContext TextureContext { get; private set; }
    public AbstractEditableAssetContext EditableContext { get; private set; }
    private readonly Action? saveChangesDelegate;

    public BaseEditorContext(
        DraggableTextureContext textureContext,
        AbstractEditableAssetContext editableContext,
        Action? saveChanges
    )
    {
        TextureContext = textureContext;
        EditableContext = editableContext;
        saveChangesDelegate = saveChanges;

        EditableContext.saveChangesDelegate = saveChangesDelegate;

        TextureContext.Dragged += EditableContext.SetSpriteIndex;
        TextureContext.TextureChanged += EditableContext.SetTexture;
        EditableContext.BoundsProviderChanged += TextureContext.OnEditorBoundsProviderChanged;

        EditableContext.SetTexture(TextureContext, TextureContext.Selected);
    }

    public void SaveChanges()
    {
        saveChangesDelegate?.Invoke();
    }
}
