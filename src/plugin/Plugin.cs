using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DressMySlugcat.Hooks;
using System.IO;
using HUD;
using System.Runtime.Serialization.Json;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Threading;
using BepInEx.Logging;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DressMySlugcat
{
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

        private void OnEnable()
        {
            try
            {
                if (IsInit) return;
                IsInit = true;

                Logger = base.Logger;

                On.Menu.MainMenu.ctor += MainMenu_ctor;

                SpriteDefinitions.Init();
                AtlasHooks.Init();
                PlayerGraphicsHooks.Init();
                MenuHooks.Init();
                PauseMenuHooks.Init();
                On.RainWorld.OnModsEnabled += RainWorld_OnModsEnabled;
                On.RainWorld.OnModsInit += RainWorld_OnModsInit;
                MachineConnector.SetRegisteredOI(BaseName, Options = new DMSOptions());

                Debug.Log($"Plugin DressMySlugcat is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                MachineConnector.SetRegisteredOI(BaseName, Options = new DMSOptions());
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