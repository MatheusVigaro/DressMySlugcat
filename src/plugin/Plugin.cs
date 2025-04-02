using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Collections.Generic;
using DressMySlugcat.Hooks;
using BepInEx.Logging;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DressMySlugcat
{
    [BepInDependency("henpemaz.rainmeadow", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(BaseName, "Dress My Slugcat", "1.0.0")]
    public partial class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null!;

        public static ProcessManager.ProcessID FancyMenu => new ProcessManager.ProcessID("FancyMenu", register: true);
        public const string BaseName = "dressmyslugcat";

        public static DMSOptions Options;

        private bool IsInit;
        private bool IsPostInit;

        private void Awake()
        {
            SpriteDefinitions.Init();
        }

        private void OnEnable()
        {
            try
            {
                Logger = base.Logger;
                Logger.LogInfo("Loading plugin DressMySlugcat");

                On.RainWorld.OnModsEnabled += RainWorld_OnModsEnabled;
                On.RainWorld.OnModsInit += RainWorld_OnModsInit;

                Logger.LogInfo("Plugin DressMySlugcat is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
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
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
                Debug.LogException(ex);
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
                Debug.LogException(ex);
            }
        }
    }
}