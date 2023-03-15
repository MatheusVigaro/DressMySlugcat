using HUD;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace DressMySlugcat.Hooks
{
    public class MenuHooks
    {
        public static void Init()
        {
            On.Menu.MainMenu.ctor += MainMenu_ctor;
            IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
        }

        private static void ProcessManager_PostSwitchMainProcess(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                     i => i.MatchLdfld<ProcessManager>("oldProcess"),
                                                     i => i.MatchLdarg(0),
                                                     i => i.MatchLdfld<ProcessManager>("currentMainLoop"),
                                                     i => i.MatchCallOrCallvirt<MainLoopProcess>("CommunicateWithUpcomingProcess")))
            {
                return;
            }

            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
            {
                if (ID == Plugin.FancyMenu)
                {
                    self.currentMainLoop = new FancyMenu(self);
                }
            });

        }

        private static void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig(self, manager, showRegionSpecificBkg);

            float buttonWidth = Menu.MainMenu.GetButtonWidth(self.CurrLang);
            Vector2 pos = new Vector2(683f - buttonWidth / 2f, 0f);
            Vector2 size = new Vector2(buttonWidth, 30f);
            
            self.AddMainMenuButton(new SimpleButton(self, self.pages[0], "GET FANCY", "GETFANCY", pos, size), () => {
                manager.RequestMainProcessSwitch(Plugin.FancyMenu);
                self.PlaySound(SoundID.MENU_Switch_Page_In);
            }, 0);
        }
    }
}