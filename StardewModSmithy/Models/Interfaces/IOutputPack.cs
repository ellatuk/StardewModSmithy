namespace StardewModSmithy.Models.Interfaces;

public interface IOutputPack
{
    public void Save();
    public void Load();
    public OutputManifest Manifest { get; }
}
