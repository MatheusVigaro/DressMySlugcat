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
        public static readonly DMSOptions Instance = new();
        public readonly Configurable<bool> LoadInactiveMods;
        public readonly Configurable<bool> DefaultMeadowSkins;
        public OpCheckBox mpBox;

        public DMSOptions()
        {
            LoadInactiveMods = config.Bind("LoadInactiveMods", false);
            DefaultMeadowSkins = config.Bind("DefaultMeadowSkins", true);
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
                new OpLabel(45f, 540f, "Load Inactive Mods"),

                this.mpBox = new OpCheckBox(DefaultMeadowSkins, 10, 500) { description = "Requires the Rain Meadow mod. If disabled, all other players skins are treated as player 1" },
                new OpLabel(45f, 500f, "Default skins for Rain Meadow players")
            };
            opTab.AddItems(elements);
        }


        public override void Update()
        {
            if (MeadowCompatibility.meadowEnabled)
                this.mpBox.greyedOut = false;
            else
                this.mpBox.greyedOut = true;
        }

    }
}