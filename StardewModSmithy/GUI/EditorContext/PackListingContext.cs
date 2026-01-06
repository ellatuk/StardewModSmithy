using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Models;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.GUI.EditorContext;

internal partial record PackDisplayEntry(IOutputPack Pack)
{
    public string PackTitle => $"{Pack.Manifest.Name} ({Pack.Manifest.UniqueID})";

    public string PackName
    {
        get => Pack.Manifest.Name;
        set
        {
            Pack.Manifest.Name = value;
            OnPropertyChanged(new(nameof(PackName)));
        }
    }

    public string PackDescription
    {
        get => Pack.Manifest.Description;
        set
        {
            Pack.Manifest.Description = value;
            OnPropertyChanged(new(nameof(PackDescription)));
        }
    }

    [Notify]
    public bool isExpanded = false;

    public string NexusID
    {
        get => Pack.Manifest.NexusID;
        set
        {
            Pack.Manifest.NexusID = value;
            OnPropertyChanged(new(nameof(NexusID)));
        }
    }

    public bool IsLoaded => ModEntry.ModRegistry.IsLoaded(Pack.Manifest.UniqueID);

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
                outputPackContentPatcher.Save
            );
        }
        else
        {
            ModEntry.Log($"Editor not implemented for this pack!", LogLevel.Error);
        }
    }
}

internal partial record PackListingContext(TextureAssetGroup TextureAssetGroup, List<IOutputPack> EditablePacks)
{
    private readonly List<PackDisplayEntry> packDisplayList = EditablePacks
        .Select(pack => new PackDisplayEntry(pack))
        .ToList();

    public IEnumerable<PackDisplayEntry> PackDisplayList => packDisplayList;

    [Notify]
    private string newModName = string.Empty;

    public string NewModErrorMessage
    {
        get
        {
            if (string.IsNullOrEmpty(NewModName))
            {
                return I18n.Message_CreateMod_NeedName();
            }
            string uniqueID = MakeUniqueID(ModEntry.Config.GetAuthorName());
            if (!IsValidUniqueID(uniqueID))
            {
                return I18n.Message_CreateMod_IdNotUnique(uniqueID);
            }
            return string.Empty;
        }
    }

    public float NewModErrorOpacity => string.IsNullOrEmpty(NewModErrorMessage) ? 1f : 0.5f;

    internal static PackListingContext? Initialize()
    {
        TextureAssetGroup textureAssetGroup = TextureAssetGroup.FromSourceDir("furniture");
        if (textureAssetGroup.GatheredTextures.Count == 0)
        {
            Game1.addHUDMessage(HUDMessage.ForCornerTextbox(I18n.Message_PutTexture(Consts.EDITING_INPUT)));
            ModEntry.Log(
                I18n.Message_PutTexture(Path.Combine(ModEntry.DirectoryPath, Consts.EDITING_INPUT)),
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
        string authorName = ModEntry.Config.AuthorName;
        string uniqueID = MakeUniqueID(authorName);
        if (!IsValidUniqueID(uniqueID))
        {
            return;
        }
        OutputManifest manifest = new()
        {
            Author = authorName,
            Name = NewModName,
            UniqueID = uniqueID,
        };
        OutputPackContentPatcher outputPackContentPatcher = new(manifest)
        {
            TextureAssetGroup = TextureAssetGroup,
            FurniAsset = new FurnitureAsset(),
        };
        outputPackContentPatcher.InitializeFurnitureAsset([]);
        PackDisplayEntry packDisplay = new(outputPackContentPatcher);
        packDisplayList.Add(packDisplay);
        PropertyChanged?.Invoke(this, new(nameof(PackDisplayList)));
        packDisplay.ShowEditingMenu();
    }

    private string MakeUniqueID(string authorName) =>
        string.Concat(Sanitize.UniqueID(authorName), '.', Sanitize.UniqueID(NewModName));

    private bool IsValidUniqueID(string uniqueID)
    {
        if (
            ModEntry.ModRegistry.IsLoaded(uniqueID)
            || EditablePacks.Any(output => output.Manifest.UniqueID.Equals(uniqueID))
        )
        {
            return false;
        }

        return true;
    }
}
