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
        public Dictionary<string, FAtlasElement> LeftElements = new();
        public Dictionary<string, FAtlasElement> RightElements = new();
        public Dictionary<string, FAtlasElement> TrimmedElements = new();
        public List<FAtlas> Atlases = new();
        public List<string> AvailableSpriteNames = new();
        public List<string> AvailableAsymmetrySpriteNames = new();
        public Dictionary<string, Color> DefaultColors = new();
        public CustomTail DefaultTail = new();

        public List<SpriteDefinitions.AvailableSprite> AvailableSprites => SpriteDefinitions.AvailableSprites.Where(x => AvailableSpriteNames.Contains(x.Name)).ToList();
        public bool HasAsymmetry(string name) => AvailableAsymmetrySpriteNames.Contains(name);

        public static SpriteSheet Get(string id) => Plugin.SpriteSheets.FirstOrDefault(x => x.ID == id);
        public static SpriteSheet GetDefault() => Get(DefaultName);
        public static SpriteSheet GetEmpty() => Get(EmptyName);

        public static readonly string DefaultName = "rainworld.default";
        public static readonly string EmptyName = "dressmyslugcat.empty";
        public static readonly string LeftPrefix = "Left";
        public static readonly string RightPrefix = "Right";

        public static FAtlas EmptyAtlas;
        public static FAtlasElement EmptyElement;

        public static FAtlas TailAtlas;
        public static FAtlasElement TailElement;

        public static Texture2D RainWorldTexture;

        public void ParseAtlases()
        {
            foreach (FAtlas atlas in Atlases)
            {
                foreach (var element in atlas.elements)
                {
                    var name = element.name.Substring(Prefix.Length);

                    if (name.StartsWith(LeftPrefix))
                    {
                        LeftElements[name.Substring(LeftPrefix.Length)] = element;
                    }
                    else if (name.StartsWith(RightPrefix))
                    {
                        RightElements[name.Substring(RightPrefix.Length)] = element;
                    }
                    else
                    {
                        Elements[name] = element;
                        if (SpriteDefinitions.AvailableSprites.Any(x => element.name.EndsWith(x.GallerySprite, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            TrimmedElements[name] = TrimElement(element);
                        }
                    }
                }
            }

            foreach (var group in SpriteDefinitions.AvailableSprites)
            {
                var required = group.RequiredSprites;
                if (required.All(sprite => Elements.Keys.Contains(sprite)))
                {
                    AvailableSpriteNames.Add(group.Name);
                }
                if (required.All(sprite => LeftElements.ContainsKey(sprite) && RightElements.ContainsKey(sprite)))
                {
                    AvailableAsymmetrySpriteNames.Add(group.Name);
                }   
            }
        }

        public static FAtlasElement TrimElement(FAtlasElement element)
        {
            var texture = element.atlas.texture as Texture2D;

            if (texture.name == "rainWorld")
            {
                if (RainWorldTexture == null)
                {
                    RainWorldTexture = new Texture2D(texture.width, texture.height, texture.format, false);
                    Graphics.ConvertTexture(texture, RainWorldTexture);
                }

                texture = RainWorldTexture;
            }

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

        public static void UpdateDefaults()
        {
            if (EmptyAtlas == null)
            {
                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.clear);
                texture.Apply();
                EmptyAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.BaseName + "__emptyatlas", texture, false);
                EmptyElement = EmptyAtlas.elements[0];
            }

            if (TailAtlas == null)
            {
                var texture = new Texture2D(150, 75);
                var pixels = new Color[texture.width * texture.height];

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                texture.SetPixels(pixels);
                texture.Apply();

                TailAtlas = Futile.atlasManager.LoadAtlasFromTexture("TailTexture", texture, false);
                TailElement = TailAtlas.elements[0];
            }

            var defaults = GetDefault();

            if (defaults == null)
            {
                defaults = new SpriteSheet
                {
                    ID = DefaultName,
                    Name = "Default",
                    Author = "DressMySlugcat"
                };
                Plugin.SpriteSheets.Insert(0, defaults);
            }

            var empty = GetEmpty();
            if (empty == null)
            {
                empty = new SpriteSheet
                {
                    ID = EmptyName,
                    Name = "Empty",
                    Author = "DressMySlugcat"
                };
                Plugin.SpriteSheets.Insert(1, empty);
            }

            foreach (var definition in SpriteDefinitions.AvailableSprites)
            {
                foreach (var sprite in definition.RequiredSprites)
                {
                    var element = Futile.atlasManager.GetElementWithName(sprite);
                    defaults.Elements[sprite] = element;
                    defaults.TrimmedElements[sprite] = TrimElement(element);

                    empty.Elements[sprite] = EmptyElement;
                    empty.TrimmedElements[sprite] = EmptyElement;
                }

                if (!defaults.AvailableSpriteNames.Contains(definition.Name))
                {
                    defaults.AvailableSpriteNames.Add(definition.Name);
                }

                if (!empty.AvailableSpriteNames.Contains(definition.Name))
                {
                    empty.AvailableSpriteNames.Add(definition.Name);
                }
            }
        }
    }
}