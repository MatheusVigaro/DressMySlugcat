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
        public Dictionary<string, FAtlasElement> TrimmedElements = new();
        public List<FAtlas> Atlases = new();
        public List<string> AvailableSpriteNames = new();
        public Dictionary<string, Color> DefaultColors = new();

        public static SpriteSheet Get(string id) => Plugin.SpriteSheets.FirstOrDefault(x => x.ID == id);
        public List<SpriteDefinitions.AvailableSprite> AvailableSprites => SpriteDefinitions.AvailableSprites.Where(x => AvailableSpriteNames.Contains(x.Name)).ToList();

        public void ParseAtlases()
        {
            foreach (FAtlas atlas in Atlases)
            {
                foreach (var element in atlas.elements)
                {
                    Elements[element.name.Substring(Prefix.Length)] = element;
                    TrimmedElements[element.name.Substring(Prefix.Length)] = TrimElement(element);
                }
            }

            foreach (var group in SpriteDefinitions.AvailableSprites)
            {
                var required = group.RequiredSprites;
                if (required.All(sprite => Elements.Keys.Contains(sprite)))
                {
                    AvailableSpriteNames.Add(group.Name);
                }
            }
        }

        private FAtlasElement TrimElement(FAtlasElement element)
        {
            var texture = element.atlas.texture as Texture2D;

            var pos = Vector2Int.RoundToInt(element.uvRect.position * element.atlas.textureSize);
            var size = Vector2Int.RoundToInt(element.uvRect.size * element.atlas.textureSize);

            var bottomLeft = pos + size;
            var topRight = new Vector2Int(0, 0);

            for (var x = pos.x; x <= pos.x + size.x; x++)
            {
                for (var y = pos.y; y <= pos.y + size.y; y++)
                {
                    if (texture.GetPixel(x, y).a != 0)
                    {
                        if (x < bottomLeft.x)
                        {
                            bottomLeft.x = x;
                        }
                        if (y < bottomLeft.y)
                        {
                            bottomLeft.y = y;
                        }
                        if (x > topRight.x)
                        {
                            topRight.x = x;
                        }
                        if (y > topRight.y)
                        {
                            topRight.y = y;
                        }
                    }
                }
            }

            topRight.x++;
            topRight.y++;

            return element.atlas.CreateUnnamedElement(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        }
    }
}