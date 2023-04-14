using HUD;
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
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TriangleMesh;
using Color = UnityEngine.Color;
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
            On.RoomCamera.SpriteLeaser.ctor += SpriteLeaser_ctor;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            IL.RoomCamera.SpriteLeaser.Update += SpriteLeaser_Update;
            IL.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            IL.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSpritesTail;
        }

        private static void SpriteLeaser_ctor(On.RoomCamera.SpriteLeaser.orig_ctor orig, RoomCamera.SpriteLeaser sLeaser, IDrawable obj, RoomCamera rCam)
        {
            orig(sLeaser, obj, rCam);

            if (obj is not PlayerGraphics playerGraphics)
            {
                return;
            }

            if (!PlayerGraphicsData.TryGetValue(playerGraphics, out var playerGraphicsData) || playerGraphicsData == null)
            {
                InitiateCustomGraphics(playerGraphics);
            }

            if (sLeaser.sprites[2] is TriangleMesh tail && PlayerGraphicsData.TryGetValue(playerGraphics, out playerGraphicsData))
            {
                if (playerGraphicsData.Customization.CustomTail.IsCustom)
                {
                    sLeaser.sprites[2].RemoveFromContainer();

                    Triangle[] array = new Triangle[(playerGraphics.tail.Length - 1) * 4 + 1];
                    for (int i = 0; i < playerGraphics.tail.Length - 1; i++)
                    {
                        int num = i * 4;
                        for (int j = 0; j < 4; j++)
                        {
                            array[num + j] = new Triangle(num + j, num + j + 1, num + j + 2);
                        }
                    }
                    array[(playerGraphics.tail.Length - 1) * 4] = new Triangle((playerGraphics.tail.Length - 1) * 4, (playerGraphics.tail.Length - 1) * 4 + 1, (playerGraphics.tail.Length - 1) * 4 + 2);
                    tail = new TriangleMesh("Futile_White", array, tail.customColor, false);
                    sLeaser.sprites[2] = tail;
                    playerGraphicsData.tailRef = tail;

                    rCam.ReturnFContainer("Midground").AddChild(tail);
                    tail.MoveBehindOtherNode(sLeaser.sprites[4]);
                }

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

            playerGraphics.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
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

        private static void PlayerGraphics_DrawSpritesTail(ILContext il)
        {
            var cursor = new ILCursor(il);

            try
            {
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(2),
                                                         i => i.MatchLdloc(1),
                                                         i => i.MatchLdcR4(0.5f)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (first)");
                }

                var label = cursor.DefineLabel();
                cursor.MarkLabel(label);

                if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdcR4(6),
                                                        i => i.MatchStloc(7)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (second)");
                }

                cursor.MoveAfterLabels();

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldarg_3);
                cursor.Emit(OpCodes.Ldarg, 4);
                cursor.EmitDelegate((PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos) =>
                {
                    if (sLeaser.sprites[2] is not TriangleMesh tailSprite)
                    {
                        return false;
                    }

                    if (!PlayerGraphicsData.TryGetValue(self, out var playerGraphicsData))
                    {
                        return false;
                    }

                    if (!playerGraphicsData.Customization.CustomTail.IsCustom || !playerGraphicsData.TailIntegrity(sLeaser))
                    {
                        return false;
                    }

                    float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
                    float num3 = 1f - 0.2f * self.malnourished;
                    float num4 = self.tail[0].rad;

                    Vector2 val = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                    Vector2 val2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                    if (self.player.aerobicLevel > 0.5f)
                    {
                        val += Custom.DirVec(val2, val) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(0.5f, 1f, self.player.aerobicLevel) * 0.5f;

                    }
                    Vector2 val4 = (val2 * 3f + val) / 4f;

                    for (int i = 0; i < self.tail.Length; i++)
                    {
                        Vector2 val5 = Vector2.Lerp(self.tail[i].lastPos, self.tail[i].pos, timeStacker);
                        Vector2 val6 = val5 - val4;
                        Vector2 normalized = val6.normalized;
                        Vector2 val7 = Custom.PerpendicularVector(normalized);
                        float num5 = Vector2.Distance(val5, val4) / 5f;
                        if (i == 0)
                        {
                            num5 = 0f;
                        }
                        tailSprite.MoveVertice(i * 4, val4 - val7 * num4 * num3 + normalized * num5 - camPos);
                        tailSprite.MoveVertice(i * 4 + 1, val4 + val7 * num4 * num3 + normalized * num5 - camPos);
                        if (i < self.tail.Length - 1)
                        {
                            tailSprite.MoveVertice(i * 4 + 2, val5 - val7 * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
                            tailSprite.MoveVertice(i * 4 + 3, val5 + val7 * self.tail[i].StretchedRad * num3 - normalized * num5 - camPos);
                        }
                        else
                        {
                            tailSprite.MoveVertice(i * 4 + 2, val5 - camPos);
                        }
                        num4 = self.tail[i].StretchedRad;
                        val4 = val5;
                    }
                    return true;
                });

                cursor.Emit(OpCodes.Brtrue, label);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for PlayerGraphics_DrawSpritesTail!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
            }
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
                    #region SpriteColors
                    for (var i = 0; i < sLeaser.sprites.Length && i < playerGraphicsData.SpriteNames.Length; i++)
                    {
                        if (playerGraphicsData.IsArtificer && playerGraphicsData.SpriteNames[i].StartsWith("FaceB"))
                        {
                            playerGraphicsData.SpriteNames[i] = "Face" + (sLeaser.sprites[i].scaleX < 0f ? "C" : "D") + playerGraphicsData.SpriteNames[i].Substring(5);
                        }

                        if (sLeaser.sprites[i] != null)
                        {
                            if (playerGraphicsData.SpriteReplacements.TryGetValue(playerGraphicsData.SpriteNames[i], out var replacement))
                            {
                                sLeaser.sprites[i].element = replacement;
                            }

                            if (playerGraphicsData.SpriteColors.TryGetValue(playerGraphicsData.SpriteNames[i], out var customColor))
                            {
                                if (playerGraphics.malnourished > 0f)
                                {
                                    float num = (playerGraphics.player.Malnourished ? playerGraphics.malnourished : Mathf.Max(0f, playerGraphics.malnourished - 0.005f));
                                    customColor = Color.Lerp(customColor, Color.gray, 0.4f * num);
                                }

                                sLeaser.sprites[i].color = playerGraphics.HypothermiaColorBlend(customColor);
                            }
                        }
                    }
                    #endregion

                    #region TailColor
                    if (playerGraphicsData.tailColor != default && playerGraphicsData.tailColor.a != 0 && sLeaser.sprites[2] != null)
                    {
                        var color = playerGraphicsData.tailColor;
                        if (playerGraphics.malnourished > 0f)
                        {
                            float num = (playerGraphics.player.Malnourished ? playerGraphics.malnourished : Mathf.Max(0f, playerGraphics.malnourished - 0.005f));
                            color = Color.Lerp(color, Color.gray, 0.4f * num);
                        }

                        sLeaser.sprites[2].color = playerGraphics.HypothermiaColorBlend(color);
                    }
                    #endregion

                    #region GillColors
                    if (playerGraphics.gills != null)
                    {
                        var gills = playerGraphics.gills;
                        for (int num = gills.startSprite + gills.scalesPositions.Length - 1; num >= gills.startSprite; num--)
                        {
                            sLeaser.sprites[num].color = sLeaser.sprites[3].color;
                        }
                    }
                    #endregion
                }
            });
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
            playerGraphicsData.Customization = customization;

            if (customization.CustomTail.IsCustom)
            {
                var length = customization.CustomTail.EffectiveLength;
                var wideness = customization.CustomTail.EffectiveWideness;
                var roundness = customization.CustomTail.EffectiveRoundness;
                //var offset = customization.CustomTail.EffectiveOffset;
                var pup = self.player.playerState.isPup;

                self.tail = new TailSegment[length];
                for (var i = 0; i < length; i++)
                {
                    self.tail[i] = new TailSegment(self, Mathf.Lerp(6f, 1f, Mathf.Pow((float)(i + 1) / (float)length, wideness)) * (1f + Mathf.Sin((float)i / (float)length * (float)Math.PI) * roundness), (float)((i == 0) ? 4 : 7) * (pup ? 0.5f : 1f), (i > 0) ? self.tail[i - 1] : null, 0.85f, 1f, (i == 0) ? 1f : 0.5f, true);
                }

                var bp = self.bodyParts.ToList();
                bp.RemoveAll(x => x is TailSegment);
                bp.AddRange(self.tail);
                self.bodyParts = bp.ToArray();
            }

            playerGraphicsData.IsArtificer = self.player.slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer;

            if (customization.CustomTail.Color != default && customization.CustomTail.Color.a != 0 && customization.CustomTail.Color != Utils.DefaultBodyColor(name))
            {
                playerGraphicsData.tailColor = customization.CustomTail.Color;
            }

            playerGraphicsData.gillEffectColor = Utils.DefaultExtraColor(name);

            foreach (var customSprite in customization.CustomSprites)
            {

                Color customColor = default;
                if (customSprite.Color != default && customSprite.Color.a != 0 && customSprite.Color != Utils.DefaultColorForSprite(name, customSprite.Sprite))
                {
                    customColor = customSprite.Color;
                }

                if (customColor != default && customSprite.Sprite == "GILLS")
                {
                    playerGraphicsData.gillEffectColor = customColor;
                }

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

                            if (customSprite.SpriteSheetID != SpriteSheet.DefaultName)
                            {
                                playerGraphicsData.SpriteReplacements[specificSprite] = sheet.Elements[sprite];
                                if (customColor != default)
                                {
                                    playerGraphicsData.SpriteColors[sheet.Elements[sprite].name] = customColor;
                                }
                            }

                            if (customColor != default)
                            {
                                playerGraphicsData.SpriteColors[specificSprite] = customColor;
                            }
                        }
                    }
                }
            }
        }
    }
}