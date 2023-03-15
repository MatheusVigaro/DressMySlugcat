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

namespace DressMySlugcat
{

    public class SpriteSheet
    {
        public string ID;
        public string Name;
        public string Author;

        public string Prefix;

        public Dictionary<string, FAtlasElement> Elements = new();
        public List<FAtlas> Atlases = new();
    }
}