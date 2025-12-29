using System.Diagnostics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.GUI;
using StardewModSmithy.GUI.EditorContext;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;
using StardewValley;

namespace StardewModSmithy;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.StardewModSmithy";
    private static IMonitor? mon;
    internal static Func<string, IAssetName> ParseAssetName = null!;
    internal static string DirectoryPath = null!;
    internal static IModContentHelper ModContent = null!;

    public static string ContentPatcherVersion { get; internal set; } = "2.1.0";

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        ParseAssetName = helper.GameContent.ParseAssetName;
        DirectoryPath = helper.DirectoryPath;
        ModContent = helper.ModContent;

        Directory.CreateDirectory(Path.Combine(DirectoryPath, Consts.EDITING_INPUT));
        Directory.CreateDirectory(Path.Combine(DirectoryPath, Consts.EDITING_OUTPUT));

        helper.ConsoleCommands.Add("sms-show", "show smithy menu to edit your mods.", ConsoleShowSmithy);
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        EditorMenuManager.Register(Helper);
        if (Helper.ModRegistry.Get("Pathoschild.ContentPatcher") is IManifest contentPatcher)
        {
            ContentPatcherVersion = contentPatcher.Version.ToString();
        }
    }

    private void ConsoleShowSmithy(string cmd, string[] args)
    {
        EditorMenuManager.ShowPackListing();
    }

    public static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    internal static void WriteJson(string targetPath, string fileName, object content)
    {
        File.WriteAllText(
            Path.Combine(targetPath, fileName),
            JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings)
        );
    }

    internal static T? ReadJson<T>(string targetPath, string fileName)
    {
        string targetFile = Path.Combine(targetPath, fileName);
        if (!File.Exists(targetFile))
            return default;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(targetFile));
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }
}
