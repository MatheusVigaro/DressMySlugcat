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
using Menu;
using RWCustom;
using Menu.Remix.MixedUI;

namespace DressMySlugcat
{

    public class DMSOptions : OptionInterface
    {
        public readonly Configurable<bool> LoadInactiveMods;

        public DMSOptions()
        {
            LoadInactiveMods = config.Bind("LoadInactiveMods", false);
        }

        public override string ValidationString()
        {
            return base.ValidationString() + (LoadInactiveMods?.Value ?? false ? " LI" : "");
        }

        private UIelement[] elements;

        public override void Initialize()
        {
            var opTab = new OpTab(this, "Settings");
            Tabs = new[] { opTab };

            elements = new UIelement[] {
                new OpCheckBox(LoadInactiveMods, 10, 540),
                new OpLabel(45f, 540f, "Load Inactive Mods")
            };
            opTab.AddItems(elements);
        }

    }
}