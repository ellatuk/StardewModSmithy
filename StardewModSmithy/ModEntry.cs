using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.GUI;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;
using StardewValley;
using StardewValley.Menus;

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
    internal static string StagingDirectoryPath = null!;
    internal static string ModCreditString = null!;
    internal static IModContentHelper ModContent = null!;
    internal static ModConfig Config = null!;
    internal static IModRegistry ModRegistry = null!;

    // https://gist.github.com/Shockah/ec111245868ee9b7dbf2ca2928dd2896
    #region execute command
    private static Action<string>? AddToRawCommandQueue = null;

    private static Action<string>? Make_AddToDrawCommandQueue()
    {
        var scoreType = Type.GetType("StardewModdingAPI.Framework.SCore, StardewModdingAPI")!;
        var commandQueueType = Type.GetType("StardewModdingAPI.Framework.CommandQueue, StardewModdingAPI")!;
        var scoreGetter = scoreType
            .GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetGetMethod(true)!;
        var rawCommandQueueField = scoreType.GetField(
            "RawCommandQueue",
            BindingFlags.NonPublic | BindingFlags.Instance
        )!;
        var commandQueueAddMethod = commandQueueType.GetMethod("Add")!;
        var dynamicMethod = new DynamicMethod("AddToRawCommandQueue", null, [typeof(string)]);
        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Call, scoreGetter);
        il.Emit(OpCodes.Ldfld, rawCommandQueueField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, commandQueueAddMethod);
        il.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<Action<string>>();
    }
    #endregion

    public static bool IsContentPatcherLoaded { get; private set; } = false;
    public static string ContentPatcherVersion { get; internal set; } = "2.1.0";

    private Rectangle titleMenuButtonBounds = new Rectangle(64, 64, 60, 80);
    private float titleMenuButtonScale = 1f;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        Config = helper.ReadConfig<ModConfig>();
        Config.doWriteConfig = helper.WriteConfig;
        ModRegistry = helper.ModRegistry;
        ModCreditString = $"by {ModManifest.Name} ({ModManifest.Version}) at ";

        ParseAssetName = helper.GameContent.ParseAssetName;
        DirectoryPath = helper.DirectoryPath;
        StagingDirectoryPath = string.Concat(DirectoryPath, ".Staging");
        ModContent = helper.ModContent;

        Directory.CreateDirectory(Path.Combine(DirectoryPath, Consts.EDITING_INPUT));
        Directory.CreateDirectory(Path.Combine(DirectoryPath, Consts.EDITING_OUTPUT));
        Directory.CreateDirectory(StagingDirectoryPath);

        helper.ConsoleCommands.Add("sms-show", "show smithy menu to edit your mods.", ConsoleShowWorkspace);
        helper.ConsoleCommands.Add("sms-pack", "pack a folder of loose textures", ConsolePackTexture);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        helper.Events.Input.CursorMoved += OnCursorMoved;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!IsTitleMenuButtonActive())
            return;
        e.SpriteBatch.Draw(
            Game1.mouseCursors,
            titleMenuButtonBounds.Center.ToVector2(),
            new Rectangle(631, 1968, 15, 20),
            Color.White,
            0f,
            new(7.5f, 10f),
            titleMenuButtonScale * 4,
            SpriteEffects.None,
            1f
        );
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!IsTitleMenuButtonActive())
            return;
        if (titleMenuButtonBounds.Contains(e.NewPosition.ScreenPixels))
        {
            titleMenuButtonScale = 1.2f;
        }
        else
        {
            titleMenuButtonScale = 1f;
        }
    }

    private static bool IsTitleMenuButtonActive()
    {
        return !Context.IsWorldReady
            && Game1.activeClickableMenu is TitleMenu titleM
            && TitleMenu.subMenu == null
            && titleM.titleInPosition
            && !titleM.isTransitioningButtons
            && !EditorMenuManager.showWorkspaceNextTick.Value;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Config.ShowWorkspaceKey.JustPressed())
        {
            EditorMenuManager.ShowWorkspace();
        }
        if (IsTitleMenuButtonActive() && titleMenuButtonScale > 1f && Game1.didPlayerJustLeftClick())
        {
            titleMenuButtonScale = 1f;
            EditorMenuManager.showWorkspaceNextTick.Value = true;
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        EditorMenuManager.Register(Helper);
        if (Helper.ModRegistry.Get("Pathoschild.ContentPatcher") is IModInfo contentPatcher)
        {
            ContentPatcherVersion = contentPatcher.Manifest.Version.ToString();
            IsContentPatcherLoaded = true;
        }
        AddToRawCommandQueue = Make_AddToDrawCommandQueue();
    }

    private static void ExecuteCommand(string command)
    {
        AddToRawCommandQueue?.Invoke(command);
    }

    internal static void PatchReload(string targetPath, string uniqueId)
    {
        if (!IsContentPatcherLoaded)
            return;

        if (!Config.AutoSymlinkAndPatchReload)
            return;

        if (ModRegistry.IsLoaded(uniqueId))
        {
            string cmd = string.Concat("patch reload ", uniqueId);
            Log($"Trying '{cmd}'", LogLevel.Info);
            ExecuteCommand(cmd);
        }
        else
        {
            string symlinkPath = Path.Combine(StagingDirectoryPath, Path.GetFileName(targetPath));
            FileInfo symlinkPathInfo = new(symlinkPath);
            if (symlinkPathInfo.LinkTarget is null && !symlinkPathInfo.Exists)
            {
                Directory.CreateSymbolicLink(symlinkPath, targetPath);
                LogOnce($"Restart the game to enable automatic patch reload on '{uniqueId}'", LogLevel.Info);
            }
            else if (symlinkPathInfo.LinkTarget == targetPath)
            {
                LogOnce(
                    $"'{symlinkPath}' exists but '{uniqueId}' is not loaded, please restart the game to allow automatic patch reload",
                    LogLevel.Warn
                );
            }
            else
            {
                LogOnce(
                    $"'{symlinkPath}' exists and does not link to '{targetPath}', please remove the folder, save again, then restart the game to allow automatic patch reload",
                    LogLevel.Error
                );
            }
        }
    }

    private void ConsoleShowWorkspace(string cmd, string[] args)
    {
        EditorMenuManager.ShowWorkspace();
    }

    private void ConsolePackTexture(string cmd, string[] args)
    {
        if (!ArgUtility.TryGet(args, 0, out string subdir, out string error))
        {
            Log(error, LogLevel.Error);
            return;
        }
        SpritePacker.Pack(subdir);
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

    internal static void WriteJson(string targetFile, object content)
    {
        File.WriteAllText(
            targetFile,
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

    internal static T? ReadJson<T>(string targetFile)
    {
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
