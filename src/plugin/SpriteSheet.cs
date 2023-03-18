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
        public List<string> AvailableSprites = new();

        public static Dictionary<string, List<string>> RequiredSprites = new();

        public SpriteSheet() {
            if (RequiredSprites.Count == 0)
            {
                RequiredSprites.Add("BODY", new List<string> { "BodyA" });
                RequiredSprites.Add("HIPS", new List<string> { "BodyA" });
                RequiredSprites.Add("LEGS", new List<string> { "LegsA0", "LegsA1", "LegsA2", "LegsA3", "LegsA4", "LegsA5", "LegsA6", "LegsAAir0", "LegsAAir1", "LegsAClimbing0", "LegsAClimbing1", "LegsAClimbing2", "LegsAClimbing3", "LegsAClimbing4", "LegsAClimbing5", "LegsAClimbing6", "LegsACrawling0", "LegsACrawling1", "LegsACrawling2", "LegsACrawling3", "LegsACrawling4", "LegsACrawling5", "LegsAOnPole0", "LegsAOnPole1", "LegsAOnPole2", "LegsAOnPole3", "LegsAOnPole4", "LegsAOnPole5", "LegsAOnPole6", "LegsAPole", "LegsAVerticalPole", "LegsAWall" });
                RequiredSprites.Add("HEAD", new List<string> { "HeadA0", "HeadA1", "HeadA2", "HeadA3", "HeadA4", "HeadA5", "HeadA6", "HeadA7", "HeadA8", "HeadA9", "HeadA10", "HeadA11", "HeadA12", "HeadA13", "HeadA14", "HeadA15", "HeadA16", "HeadA17" });
                RequiredSprites.Add("FACE", new List<string> { "FaceA0", "FaceA1", "FaceA2", "FaceA3", "FaceA4", "FaceA5", "FaceA6", "FaceA7", "FaceA8", "FaceB0", "FaceB1", "FaceB2", "FaceB3", "FaceB4", "FaceB5", "FaceB6", "FaceB7", "FaceB8", "FaceDead", "FaceStunned" });
                RequiredSprites.Add("ARMS", new List<string> { "PlayerArm0", "PlayerArm1", "PlayerArm2", "PlayerArm3", "PlayerArm4", "PlayerArm5", "PlayerArm6", "PlayerArm7", "PlayerArm8", "PlayerArm9", "PlayerArm10", "PlayerArm11", "PlayerArm12", "OnTopOfTerrainHand", "OnTopOfTerrainHand2" });
                RequiredSprites.Add("TAIL", new List<string> { "TailTexture" });
            }
        }

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

            foreach (var group in RequiredSprites.Keys)
            {
                var required = RequiredSprites[group];
                if (required.All(sprite => Elements.Keys.Contains(sprite)))
                {
                    AvailableSprites.Add(group);
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