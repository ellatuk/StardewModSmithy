using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Models;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy.GUI.EditorContext;

internal record PackDisplayEntry(IOutputPack Pack) : INotifyPropertyChanged
{
    public string PackTitle => $"{Pack.Manifest.Name} ({Pack.Manifest.UniqueID})";

    public string PackAuthor
    {
        get => Pack.Manifest.Author;
        set
        {
            Pack.Manifest.Author = value;
            OnPropertyChanged(new(nameof(PackAuthor)));
        }
    }

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

    public string NexusID
    {
        get => Pack.Manifest.NexusID;
        set
        {
            Pack.Manifest.NexusID = value;
            OnPropertyChanged(new(nameof(NexusID)));
        }
    }

    private bool isExpanded = false;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded && !value)
            {
                Pack.Save();
                OnPropertyChanged(new(nameof(PackTitle)));
            }
            isExpanded = value;
            OnPropertyChanged(new(nameof(IsExpanded)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(PropertyChangedEventArgs value) => PropertyChanged?.Invoke(this, value);

    public bool IsLoaded => ModEntry.ModRegistry.IsLoaded(Pack.Manifest.UniqueID);

    public void ShowEdit_Furniture()
    {
        IsExpanded = false;

        if (
            Pack is OutputPackContentPatcher outputPackContentPatcher
            && outputPackContentPatcher.TextureAssetGroup != null
        )
        {
            outputPackContentPatcher.FurniAsset ??= new();
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

    public void ShowEdit_WallFloor()
    {
        IsExpanded = false;
        if (
            Pack is OutputPackContentPatcher outputPackContentPatcher
            && outputPackContentPatcher.TextureAssetGroup != null
        )
        {
            outputPackContentPatcher.WallAndFloorAsset ??= new();
            EditorMenuManager.ShowWallpaperAndFlooringEditor(
                outputPackContentPatcher.TextureAssetGroup,
                outputPackContentPatcher.WallAndFloorAsset,
                outputPackContentPatcher.Save
            );
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

    [DependsOn(nameof(NewModName))]
    public string NewModId => MakeUniqueID(ModEntry.Config.GetAuthorName());

    public string NewModErrorMessage
    {
        get
        {
            if (string.IsNullOrEmpty(Sanitize.UniqueID(NewModName)))
            {
                return I18n.Message_CreateMod_NeedName();
            }
            string uniqueID = NewModId;
            if (!IsValidUniqueID(uniqueID))
            {
                return I18n.Message_CreateMod_IdNotUnique(uniqueID);
            }
            return string.Empty;
        }
    }

    public float NewModErrorOpacity => string.IsNullOrEmpty(NewModErrorMessage) ? 1f : 0.5f;

    [DependsOn(nameof(NewModName))]
    public string CreateButtonText => I18n.Gui_Button_Create_Mod(NewModId);

    internal static PackListingContext? Initialize()
    {
        TextureAssetGroup textureAssetGroup = TextureAssetGroup.FromSourceDir("smithy");
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
        OutputPackContentPatcher outputPackContentPatcher = new(manifest) { TextureAssetGroup = TextureAssetGroup };
        outputPackContentPatcher.InitializeFurnitureAsset([]);
        PackDisplayEntry packDisplay = new(outputPackContentPatcher);
        packDisplayList.Add(packDisplay);
        PropertyChanged?.Invoke(this, new(nameof(PackDisplayList)));
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
