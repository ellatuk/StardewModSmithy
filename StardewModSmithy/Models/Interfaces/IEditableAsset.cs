namespace StardewModSmithy.Models.Interfaces;

public interface IEditableAsset
{
    public string? Desc { get; }
    public string Target { get; }
    public string IncludeName { get; }
    public Dictionary<string, object> GetData();
    public bool GetTranslations(ref Dictionary<string, string> translations);
}
