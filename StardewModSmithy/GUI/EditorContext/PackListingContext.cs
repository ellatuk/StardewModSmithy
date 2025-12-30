using System.ComponentModel;
using StardewModdingAPI;
using StardewModSmithy.Models;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.GUI.EditorContext;

internal record PackDisplayContext(IOutputPack Pack)
{
    public string PackTitle => $"{Pack.Manifest.Name} ({Pack.Manifest.UniqueID})";
    public string PackDesc => Pack.Manifest.Desc;

    public void ShowEditingMenu()
    {
        if (
            Pack is OutputPackContentPatcher outputPackContentPatcher
            && outputPackContentPatcher.TextureAssetGroup != null
            && outputPackContentPatcher.FurniAsset != null
        )
        {
            EditorMenuManager.ShowFurnitureEditor(
                outputPackContentPatcher.TextureAssetGroup,
                outputPackContentPatcher.FurniAsset,
                outputPackContentPatcher.Save,
                Context.IsWorldReady
            );
        }
        else
        {
            ModEntry.Log($"Editor not implemented for this pack!", LogLevel.Error);
        }
    }
}

internal record PackListingContext(TextureAssetGroup TextureAssetGroup, List<IOutputPack> EditablePacks)
    : INotifyPropertyChanged
{
    private readonly List<PackDisplayContext> packDisplayList = EditablePacks
        .Select(pack => new PackDisplayContext(pack))
        .ToList();

    public event PropertyChangedEventHandler? PropertyChanged;

    public IEnumerable<PackDisplayContext> PackDisplayList => packDisplayList;

    private string newModName = "";
    public string NewModName
    {
        get => newModName;
        set
        {
            newModName = value;
            PropertyChanged?.Invoke(this, new(nameof(NewModName)));
        }
    }

    internal static PackListingContext? Initialize()
    {
        TextureAssetGroup textureAssetGroup = TextureAssetGroup.FromSourceDir("furniture");
        if (textureAssetGroup.GatheredTextures.Count == 0)
        {
            Game1.addHUDMessage(HUDMessage.ForCornerTextbox(I18n.Hud_PutTexture(Consts.EDITING_INPUT)));
            ModEntry.Log(
                I18n.Hud_PutTexture(Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT)),
                LogLevel.Warn
            );
            return null;
        }
        List<IOutputPack> outputPacks = [];
        foreach (OutputManifest manifest in OutputManifest.LoadAllFromOutputFolder())
        {
            // TODO: assume content patcher for now
            OutputPackContentPatcher outputContentPatcher = new(manifest);
            outputContentPatcher.Load();
            outputContentPatcher.TextureAssetGroup = textureAssetGroup;
            if (outputContentPatcher.EditableAssets.Any())
            {
                outputPacks.Add(outputContentPatcher);
            }
        }
        return new PackListingContext(textureAssetGroup, outputPacks);
    }

    public void CreateAndEdit()
    {
        string authorName = "Smithy";
        if (Context.IsWorldReady)
        {
            authorName = Game1.player.displayName;
        }
        OutputManifest manifest = new() { Author = authorName, Name = NewModName };
        OutputPackContentPatcher outputPackContentPatcher = new(manifest)
        {
            TextureAssetGroup = TextureAssetGroup,
            FurniAsset = new FurnitureAsset(),
        };
        outputPackContentPatcher.InitializeFurnitureAsset();
        PackDisplayContext packDisplay = new(outputPackContentPatcher);
        packDisplayList.Add(packDisplay);
        PropertyChanged?.Invoke(this, new(nameof(PackDisplayList)));
        packDisplay.ShowEditingMenu();
    }
}
