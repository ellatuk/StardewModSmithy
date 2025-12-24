namespace StardewModSmithy.GUI.EditorContext;

public sealed class BaseEditorContext
{
    public DraggableTextureContext TextureContext { get; private set; }
    public AbstractEditableAssetContext EditableContext { get; private set; }

    public BaseEditorContext(DraggableTextureContext textureContext, AbstractEditableAssetContext editableContext)
    {
        TextureContext = textureContext;
        EditableContext = editableContext;

        TextureContext.Dragged += EditableContext.SetSpriteIndex;
        TextureContext.TextureChanged += EditableContext.SetTexture;
        EditableContext.BoundsProviderChanged += TextureContext.OnEditorBoundsProviderChanged;
    }
}
