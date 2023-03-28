using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace DressMySlugcat.Hooks
{
    public class PlayerGraphicsHooks
    {
        public static ConditionalWeakTable<PlayerGraphics, PlayerGraphicsEx> PlayerGraphicsData = new();

        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            IL.RoomCamera.SpriteLeaser.Update += SpriteLeaser_Update;
            IL.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            //-- Legs behind hips
            sLeaser.sprites[4].MoveBehindOtherNode(sLeaser.sprites[1]);

            //-- Tail behind legs
            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[4]);

            //-- Gills behind face
            if (self.gills != null)
            {
                var lastSprite = sLeaser.sprites[9];
                for (int i = self.gills.startSprite + self.gills.numberOfSprites - 1; i >= self.gills.startSprite; i--)
                {
                    sLeaser.sprites[i].MoveBehindOtherNode(lastSprite);
                    lastSprite = sLeaser.sprites[i];
                }
            }

            //-- Arms behind head (can also go behind body when walking, check DrawSprites)
            sLeaser.sprites[5].MoveBehindOtherNode(sLeaser.sprites[3]);
            sLeaser.sprites[6].MoveBehindOtherNode(sLeaser.sprites[3]);
        }

        private static void PlayerGraphics_DrawSprites(ILContext il)
        {
            var cursor = new ILCursor(il);

            try
            {
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchRet()))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSprites!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for PlayerGraphics_DrawSprites!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
            }

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((RoomCamera.SpriteLeaser sLeaser) =>
            {
                if (sLeaser.drawableObject is PlayerGraphics playerGraphics && PlayerGraphicsData.TryGetValue(playerGraphics, out var playerGraphicsData))
                {
                    playerGraphicsData.SpriteNames = new string[sLeaser.sprites.Length];
                    for (var i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        playerGraphicsData.SpriteNames[i] = sLeaser.sprites[i].element.name;
                    }

                    if (playerGraphics.player.flipDirection == 1)
                    {
                        sLeaser.sprites[5].MoveBehindOtherNode(sLeaser.sprites[3]);
                        sLeaser.sprites[6].MoveBehindOtherNode(sLeaser.sprites[0]);
                    }
                    else
                    {
                        sLeaser.sprites[5].MoveBehindOtherNode(sLeaser.sprites[0]);
                        sLeaser.sprites[6].MoveBehindOtherNode(sLeaser.sprites[3]);
                    }
                }
            });
        }

        private static void SpriteLeaser_Update(ILContext il)
        {
            var cursor = new ILCursor(il);


            try
            {
                if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0),
                                                        i => i.MatchLdfld<RoomCamera.SpriteLeaser>("drawableObject"),
                                                        i => i.MatchLdarg(0),
                                                        i => i.MatchLdarg(2),
                                                        i => i.MatchLdarg(1),
                                                        i => i.MatchLdarg(3),
                                                        i => i.MatchCallOrCallvirt<IDrawable>("DrawSprites")))
                {
                    throw new Exception("Failed to match IL for SpriteLeaser_Update!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for SpriteLeaser_Update!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
            }


            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((RoomCamera.SpriteLeaser sLeaser) =>
            {
                if (sLeaser.drawableObject is PlayerGraphics playerGraphics && PlayerGraphicsData.TryGetValue(playerGraphics, out var playerGraphicsData) && playerGraphicsData != null && playerGraphicsData.SpriteReplacements != null && sLeaser.sprites != null && playerGraphicsData.SpriteNames != null)
                {
                    for (var i = 0; i < sLeaser.sprites.Length && i < playerGraphicsData.SpriteNames.Length; i++)
                    {
                        if (playerGraphicsData.IsArtificer && playerGraphicsData.SpriteNames[i].StartsWith("FaceB"))
                        {
                            playerGraphicsData.SpriteNames[i] = "Face" + (sLeaser.sprites[i].scaleX < 0f ? "C" : "D") + playerGraphicsData.SpriteNames[i].Substring(5);
                        }

                        if (playerGraphicsData.SpriteReplacements.TryGetValue(playerGraphicsData.SpriteNames[i], out var replacement))
                        {
                            if (sLeaser.sprites[i] != null)
                            {
                                sLeaser.sprites[i].element = replacement;
                            }
                        }
                    }
                }
            });
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!PlayerGraphicsData.TryGetValue(self, out var playerGraphicsData) || playerGraphicsData == null)
            {
                InitiateCustomGraphics(self);
            }

            if (sLeaser.sprites[2] is TriangleMesh tail && PlayerGraphicsData.TryGetValue(self, out playerGraphicsData))
            {
                if (playerGraphicsData.SpriteReplacements.TryGetValue("TailTexture", out var tailTexture) && tailTexture != null)
                {
                    tail.element = tailTexture;
                    for (int i = tail.vertices.Length - 1; i >= 0; i--)
                    {
                        float perc = i / 2 / (float)(tail.vertices.Length / 2);
                        Vector2 uv;
                        if (i % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (i < tail.vertices.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[i] = uv;

                    }
                }
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            InitiateCustomGraphics(self);
        }

        public static void InitiateCustomGraphics(PlayerGraphics self)
        {
            if (PlayerGraphicsData.TryGetValue(self, out var _))
            {
                PlayerGraphicsData.Remove(self);
            }

            var playerGraphicsData = new PlayerGraphicsEx();
            PlayerGraphicsData.Add(self, playerGraphicsData);

            var name = self.player.slugcatStats.name.value;

            var customization = Customization.For(self);
            foreach (var customSprite in customization.CustomSprites)
            {
                if (customSprite.SpriteSheetID == "rainworld.default")
                {
                    continue;
                }

                playerGraphicsData.IsArtificer = self.player.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer;

                var sheet = customSprite.SpriteSheet;
                if (sheet != null)
                {
                    foreach (var definition in sheet.AvailableSprites.Where(x => x.Name == customSprite.Sprite && (x.Slugcats.Count == 0 || x.Slugcats.Contains(name))))
                    {
                        foreach (var sprite in definition.RequiredSprites)
                        {
                            var specificSprite = sprite;

                            var specificReplacement = definition.SlugcatSpecificReplacements.FirstOrDefault(x => x.GenericName == sprite && ((self.player.playerState.isPup && x.Slugcat == "Slugpup") || x.Slugcat == name));
                            if (specificReplacement != null)
                            {
                                specificSprite = specificReplacement.SpecificName;
                            }

                            playerGraphicsData.SpriteReplacements[specificSprite] = sheet.Elements[sprite];
                        }
                    }
                }
            }
        }
    }
}