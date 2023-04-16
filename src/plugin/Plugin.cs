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
        public static ProcessManager.ProcessID FancyMenu => new ProcessManager.ProcessID("FancyMenu", register: true);
        public const string BaseName = "dressmyslugcat";

        private bool IsInit;
        private bool IsPostInit;
        public static bool LoadInactiveMods;

        private void OnEnable()
        {
            try
            {
                if (IsInit) return;
                IsInit = true;

                On.Menu.MainMenu.ctor += MainMenu_ctor;

                SpriteDefinitions.Init();
                AtlasHooks.Init();
                PlayerGraphicsHooks.Init();
                MenuHooks.Init();
                On.RainWorld.OnModsEnabled += RainWorld_OnModsEnabled;

                LoadInactiveMods = File.Exists(Path.Combine(Custom.RootFolderDirectory(), "dms_loadinactive.txt"));

                if (LoadInactiveMods)
                {
                    IL.ProcessManager.CreateValidationLabel += ProcessManager_CreateValidationLabel;
                }

                Debug.Log($"Plugin DressMySlugcat is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ProcessManager_CreateValidationLabel(ILContext il)
        {
            var cursor = new ILCursor(il);

            try
            {
                int mod = -1;
                if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdloc(out mod),
                                                        i => i.MatchLdfld<ModManager.Mod>(nameof(ModManager.Mod.id)),
                                                        i => i.MatchCallOrCallvirt(nameof(MachineConnector), nameof(MachineConnector.GetRegisteredOI))) || mod == -1)
                {
                    throw new Exception("Failed to match IL for ProcessManager_CreateValidationLabel!");
                }

                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Ldloc, mod);
                cursor.EmitDelegate((OptionInterface oi, ModManager.Mod mod) =>
                {
                    if (BaseName.Equals(mod.id))
                    {
                        return new DummyOptionsInterface() { mod = mod };
                    }

                    return oi;
                });
            }

            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for ProcessManager_CreateValidationLabel!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
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