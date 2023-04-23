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
    public class PauseMenuHooks
    {
        public static void Init()
        {
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
            On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        }

        private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);

            var fancyButton = new SimpleButton(self, self.pages[0], "GET FANCY", "GETFANCY", new Vector2(self.continueButton.pos.x, self.continueButton.pos.y + 38f), new Vector2(110f, 30f));
            self.pages[0].subObjects.Add(fancyButton);
        }

        private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
        {
            orig(self, sender, message);

            if (message == "GETFANCY")
            {
                self.manager.ShowDialog(new FancyMenu(self.manager, self));
                self.container.alpha = 0;
            }
        }
    }
}