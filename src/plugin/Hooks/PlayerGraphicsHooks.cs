using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class PlayerGraphicsHooks
    {
        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            var name = self.player.slugcatStats.name.value.ToLower();
            /*
            if (sLeaser.sprites[2] is TriangleMesh tail && Plugin.TailAtlases.ContainsKey(name))
            {
                var atlas = Plugin.TailAtlases[name];
                if (atlas.elements != null && atlas.elements.Count > 0)
                {
                    tail.element = atlas.elements[0];
                    for (int i = tail.verticeColors.Length - 1; i >= 0; i--)
                    {
                        float perc = i / 2 / (float)(tail.verticeColors.Length / 2);
                        Vector2 uv;
                        if (i % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (i < tail.verticeColors.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[i] = uv;
                    }
                }
            }*/
        }

        public static Dictionary<string, Dictionary<string, string>> SpriteReplacements = new();

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            var name = self.player.slugcatStats.name.value.ToLower();
            if (!SpriteReplacements.ContainsKey(name))
            {
                var startingName = "fancyslugcatslite_" + name + "_";

                Dictionary<string, string> replacements = new();
                SpriteReplacements[name] = replacements;

                foreach (var key in Futile.atlasManager._allElementsByName.Keys)
                {
                    if (key.StartsWith(startingName))
                    {
                        replacements[key.Substring(startingName.Length)] = key;
                    }
                }
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                Debug.LogWarning("a");
                var sprites = sLeaser.sprites;
            }

            var name = self.player.slugcatStats.name.value.ToLower();
            if (SpriteReplacements.ContainsKey(name))
            {
                var replacements = SpriteReplacements[name];

                foreach (var sprite in sLeaser.sprites)
                {
                    if (replacements.ContainsKey(sprite.element.name))
                    {
                        sprite.element = Futile.atlasManager.GetElementWithName(replacements[sprite.element.name]);
                    }
                }
            }
        }
    }
}