using DressMySlugcat.Hooks;

namespace DressMySlugcat;

[BepInDependency("henpemaz.rainmeadow", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(BaseName, "Dress My Slugcat", "2.1.3")]
public class Plugin : BaseUnityPlugin
{
    public static ProcessManager.ProcessID FancyMenu => new ProcessManager.ProcessID("FancyMenu", register: true);
    public const string BaseName = "dressmyslugcat";

    public static DMSOptions Options;

    private bool IsInit;
    private bool IsPostInit;

    public static new ManualLogSource Logger;

    public static void DebugLog(object ex) => Logger.LogInfo(ex);

    public static void DebugWarning(object ex) => Logger.LogWarning(ex);

    public static void DebugError(object ex) => Logger.LogError(ex);

    public static void DebugFatal(object ex) => Logger.LogFatal(ex);

    public void Awake()
    {
        Logger = base.Logger;

        SpriteDefinitions.Init();
    }

    public void OnEnable()
    {
        try
        {
            DebugLog("Loading plugin DressMySlugcat");

            On.RainWorld.OnModsEnabled += RainWorld_OnModsEnabled;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

            DebugLog("Plugin DressMySlugcat is loaded!");
        }
        catch (Exception ex)
        {
            DebugError(ex);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            MachineConnector.SetRegisteredOI(BaseName, Options = DMSOptions.Instance);
            
            if (IsInit) return;
            IsInit = true;
            
            On.Menu.MainMenu.ctor += MainMenu_ctor;

            AtlasHooks.Init();
            PlayerGraphicsHooks.Init();
            MenuHooks.Init();
            PauseMenuHooks.Init();
            NoirEars.NoirEars.LoadAtlases();
            NoirEars.NoirEars.ApplyHooks();
        }
        catch (Exception ex)
        {
            DebugError(ex);
        }
    }

    private void RainWorld_OnModsEnabled(On.RainWorld.orig_OnModsEnabled orig, RainWorld self, ModManager.Mod[] newlyEnabledMods)
    {
        orig(self, newlyEnabledMods);
        try
        {
            SpriteSheet.UpdateDefaults();
        }
        catch (Exception ex)
        {
            DebugError(ex);
        }
    }

    public static List<SpriteSheet> SpriteSheets = new();

    private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
    {
        orig(self, manager, showRegionSpecificBkg);

        try
        {
            if (IsPostInit) return;
            IsPostInit = true;

            SpriteSheet.UpdateDefaults();
            AtlasHooks.LoadAtlases();
            SaveManager.Load();
        }
        catch (Exception ex)
        {
            DebugError(ex);
        }
    }
}