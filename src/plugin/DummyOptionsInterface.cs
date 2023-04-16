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

namespace DressMySlugcat
{

    public class DummyOptionsInterface : OptionInterface
    {
        public override string ValidationString()
        {
            return base.ValidationString() + (Plugin.LoadInactiveMods ? " LI" : "");
        }
    }
}