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

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DressMySlugcat
{
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("dressmyslugcat", "Dress My Slugcat", "1.0.0")]
    public partial class Plugin : BaseUnityPlugin
    {
        public static ProcessManager.ProcessID FancyMenu => new ProcessManager.ProcessID("FancyMenu", register: true);
        public const string BaseName = "dressmyslugcat";

        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnOnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        }

        private bool IsInit;
        private bool IsPostInit;

        private void RainWorld_OnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;
                IsInit = true;

                SpriteDefinitions.Init();
                AtlasHooks.Init();
                PlayerGraphicsHooks.Init();
                MenuHooks.Init();

                Debug.Log($"Plugin DressMySlugcat is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static List<SpriteSheet> SpriteSheets = new();

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            if (IsPostInit) return;
            IsPostInit = true;

            AtlasHooks.LoadAtlases();
            SaveManager.Load();
        }
    }
}