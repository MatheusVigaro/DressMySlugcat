﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Watcher;
using static TriangleMesh;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;

namespace DressMySlugcat.Hooks
{
    public class PlayerGraphicsHooks
    {
        public static ConditionalWeakTable<PlayerGraphics, PlayerGraphicsEx> PlayerGraphicsData = new();

        public static void Init()
        {
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

            InitiateCustomGraphics(playerGraphics, sLeaser, rCam);
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

                        if (playerGraphics.player.slugcatStats.name != WatcherEnums.SlugcatStatsName.Watcher)
                            sLeaser.sprites[i].shader = FShader.defaultShader;
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
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(out _),
                                                         i => i.MatchLdloc(out _),
                                                         i => i.MatchLdcR4(0.5f)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (first)");
                }

                var label = cursor.DefineLabel();
                cursor.MarkLabel(label);

                if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdcR4(6),
                                                        i => i.MatchStloc(out _)))
                {
                    throw new Exception("Failed to match IL for PlayerGraphics_DrawSpritesTail! (second)");
                }

                cursor.MoveAfterLabels();

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldarg_2);
                cursor.Emit(OpCodes.Ldarg_3);
                cursor.Emit(OpCodes.Ldarg, 4);
                cursor.EmitDelegate((PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) =>
                {
                    if (sLeaser.sprites[2] is not TriangleMesh tailSprite)
                    {
                        return false;
                    }

                    if (!PlayerGraphicsData.TryGetValue(self, out var playerGraphicsData))
                    {
                        return false;
                    }


                    //-FB so asymmetry isn't reliant on the custom tail option
                    bool uvMapped = false;

                    //-FB force this UV to update
                    if (Customization.For(self).CustomTail.IsAsym)
                    {
                        MapTailUV(self, playerGraphicsData, tailSprite);
                        uvMapped = true;
                    }

                    /*if (!playerGraphicsData.Customization.CustomTail.IsCustom || !playerGraphicsData.TailIntegrity(sLeaser))
                    {
                        return false;
                    }*/
                    //-WW JUST STOP IF WE AREN'T CUSTOM
                    if (!playerGraphicsData.Customization.CustomTail.IsCustom)
                    {
                        return false;
                    }

                    if (!playerGraphicsData.TailIntegrity(sLeaser))
                        ReplaceTailGraphics(self, sLeaser, rCam, uvMapped);


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
                        if (i < self.tail.Length - 1 && i * 4 + 3 < tailSprite.vertices.Length)
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
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.EmitDelegate((RoomCamera.SpriteLeaser sLeaser, float timeStacker, RoomCamera rCam) =>
            {
                if (sLeaser.drawableObject is PlayerGraphics playerGraphics && PlayerGraphicsData.TryGetValue(playerGraphics, out var playerGraphicsData) && playerGraphicsData != null && playerGraphicsData.SpriteReplacements != null && sLeaser.sprites != null && playerGraphicsData.SpriteNames != null)
                {
                    if (playerGraphicsData.ScheduleForRecreation)
                    {
                        playerGraphicsData = InitiateCustomGraphics(playerGraphics, sLeaser, rCam);
                    }

                    #region SpriteColors
                    for (var i = 0; i < sLeaser.sprites.Length && i < playerGraphicsData.SpriteNames.Length; i++)
                    {
                        if (playerGraphicsData.IsArtificer && playerGraphicsData.SpriteNames[i].StartsWith("FaceB"))
                        {
                            playerGraphicsData.SpriteNames[i] = "Face" + (sLeaser.sprites[i].scaleX < 0f ? "C" : "D") + playerGraphicsData.SpriteNames[i].Substring(5);
                        }

                        if (sLeaser.sprites[i] != null)
                        {
                            string spriteName = playerGraphicsData.SpriteNames[i];
                            switch (i)
                            {
                                case 0:
                                    spriteName = "BodyA";
                                    break;
                                case 1:
                                    spriteName = "HipsA";
                                    break;
                                case 11:
                                    spriteName = "pixel";
                                    break;
                            }

                            FAtlasElement replacement = null;

                            //-- Sprite asymmetry
                            if (playerGraphics.player.bodyMode == Player.BodyModeIndex.Stand)
                            {
                                if (playerGraphics.player.bodyChunks[1].vel.x < -3f)
                                {
                                    playerGraphicsData.LeftSpriteReplacements.TryGetValue(spriteName, out replacement);
                                }
                                else if (playerGraphics.player.bodyChunks[1].vel.x > 3f)
                                {
                                    playerGraphicsData.RightSpriteReplacements.TryGetValue(spriteName, out replacement);
                                }
                            }
                            else if (playerGraphics.player.bodyMode == Player.BodyModeIndex.Crawl || playerGraphics.player.bodyMode == Player.BodyModeIndex.CorridorClimb || playerGraphics.player.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut)
                            {
                                var headPos = Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker);
                                var legsPos = Vector2.Lerp(playerGraphics.drawPositions[1, 1], playerGraphics.drawPositions[1, 0], timeStacker);
                                var bodyAngle = Custom.AimFromOneVectorToAnother(legsPos, headPos);

                                if (bodyAngle < -30 && bodyAngle > -150)
                                {
                                    playerGraphicsData.LeftSpriteReplacements.TryGetValue(spriteName, out replacement);
                                }
                                else if (bodyAngle > 30 && bodyAngle < 150)
                                {
                                    playerGraphicsData.RightSpriteReplacements.TryGetValue(spriteName, out replacement);
                                }
                            }

                            switch (i)
                            {
                                //-- Forcing arm asymmetry when enabled
                                case 5 or 7:
                                    playerGraphicsData.LeftSpriteReplacements.TryGetValue(spriteName, out replacement);
                                    break;
                                case 6 or 8:
                                    playerGraphicsData.RightSpriteReplacements.TryGetValue(spriteName, out replacement);
                                    break;
                                //-- Forcing left/right sprites based on sprite scale for the legs, head and face
                                case 3 or 4 or 9:
                                    if (sLeaser.sprites[i].scaleX < 0)
                                    {
                                        playerGraphicsData.LeftSpriteReplacements.TryGetValue(spriteName, out replacement);
                                    }
                                    else
                                    {
                                        playerGraphicsData.RightSpriteReplacements.TryGetValue(spriteName, out replacement);
                                    }
                                    break;
                            }


                            if (replacement != null || playerGraphicsData.SpriteReplacements.TryGetValue(spriteName, out replacement))
                            {
                                sLeaser.sprites[i].element = replacement;
                            }

                            //-FB fix for the custom mark (that doesn't break the game this time)
                            // custom mark is shrunk by 25x to allow more detail (vanilla is literally 1 pixel)
                            if (i == 11)
                            {
                                if (replacement != null)
                                {
                                    sLeaser.sprites[i].scale = 1.0f;
                                }
                                else
                                {
                                    sLeaser.sprites[i].scale = 5.0f;
                                }
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


        //-WW
        /*
        public static float GetSegmentRadius(int segment, int length, float wideness, float roundness, int offset, bool pup)
        {
            int i = segment;
            var k = length - i;
            float radiusWidth = 0f;

            if (k > offset)
            {
                radiusWidth = (wideness / 1.743f) + Mathf.Sqrt(((length - k) * (Mathf.Pow(wideness, 2))) / (5.5f * (length - offset)));
            }
            else
            {
                float num = ((1 + 0.04f * (offset - 6) * Mathf.Min(1, roundness - 1))) * Mathf.Sqrt((roundness * Mathf.Pow(k, 1 / roundness) * Mathf.Pow(wideness, 2f)) / offset);
                float num2 = (wideness / 1.743f) + Mathf.Sqrt(((length - offset) * (Mathf.Pow(wideness, 2))) / (5.5f * (length - offset)));
                radiusWidth = ((num > num2) && (offset <= 5) && (roundness > 2)) ? num2 - (num - num2) : num;
            }
            //offset = 0;
            //radiusWidth = (wideness / 1.743f) + Mathf.Sqrt(((length - k) * (Mathf.Pow(wideness, 2))) / (5.5f * (length - offset)));


            //Debug.Log("Is the condition true?: " + (k >= offset));
            //Debug.LogFormat("'i' is: " + i);
            //Debug.LogFormat("'k' is: " + k);
            //Debug.LogFormat("RD: " + radiusWidth);
            //Debug.LogFormat("\n");
            //self.tail[i] = new TailSegment(self, radiusWidth, (float)((i == 0) ? 4 : 7) * (pup ? 0.5f : 1f), (i > 0) ? self.tail[i - 1] : null, 0.85f, 1f, (i == 0) ? 1f : 0.5f, false);
            return radiusWidth;
            //}
        }
        */

        public static float GetSegmentRadius(int segment, int length, float wideness, float roundness, int offset, bool pup)
        {
            int i = segment;
            float segRad = Mathf.Lerp(6f, 1f, Mathf.Pow((float)(i + 1) / (float)length, wideness)) * (1f + Mathf.Sin((float)i / (float)length * (float)Math.PI) * roundness);
            return segRad;
            //Debug.LogFormat("RAD: " + segRad);
        }


        public static PlayerGraphicsEx InitiateCustomGraphics(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            //CHECK MEADOW STUFF
            if (Plugin.Options.DefaultMeadowSkins.Value && MeadowCompatibility.CheckForMeadowNonselfClient(self.owner as Player))
            {
                Debug.Log("THIS MEADOW CLIENT IS NOT US! DEFAULT SKIN FOR THEM");
                return null;
            }

            var playerGraphicsData = new PlayerGraphicsEx();
            if (PlayerGraphicsData.TryGetValue(self, out var oldData))
            {
                if (oldData.originalTail != null)
                {
                    playerGraphicsData.originalTailElement = oldData.originalTailElement;
                    playerGraphicsData.originalTailColors = oldData.originalTailColors;
                    playerGraphicsData.SpriteNames = oldData.SpriteNames;
                    playerGraphicsData.originalTail = oldData.originalTail;
                }
                PlayerGraphicsData.Remove(self);
            }

            if (playerGraphicsData.originalTail == null)
            {
                var tailSprite = sLeaser.sprites[2] as TriangleMesh;
                playerGraphicsData.originalTailElement = tailSprite.element;
                playerGraphicsData.originalTailColors = tailSprite.verticeColors;
                playerGraphicsData.originalTail = self.tail;
            }

            PlayerGraphicsData.Add(self, playerGraphicsData);

            var name = self.player.slugcatStats.name.value;

            var customization = Customization.For(self);
            playerGraphicsData.Customization = customization;
            var tailDefaults = SpriteDefinitions.GetSlugcatDefault(customization.Slugcat, customization.PlayerNumber)?.Copy();
            //-WW - IF THE CUSTOM TAIL DIDN'T PROVIDE ANY SIZE VALUES, DON'T RESIZE IT
            bool blankDefaults = false;
            if (tailDefaults != null)
                blankDefaults = tailDefaults.CustomTail.Length + tailDefaults.CustomTail.Wideness + tailDefaults.CustomTail.Roundness == 0;

            TailSegment[] oldTail;
            if (customization.CustomTail.IsCustom || (tailDefaults != null && !blankDefaults))
            {
                var length = customization.CustomTail.EffectiveLength;
                var wideness = customization.CustomTail.EffectiveWideness;
                var roundness = customization.CustomTail.EffectiveRoundness;
                //var offset = customization.CustomTail.EffectiveOffset;
                var pup = self.player.playerState.isPup;


                //WW- IF DEFAULTS EXIST FOR THIS SLUGCAT, USE THOSE TO CREATE OUR TAIL SIZE
                if (customization.CustomTail.IsCustom == false && tailDefaults != null)
                {
                    if (tailDefaults.CustomTail.Length != 0)
                        length = (int)tailDefaults.CustomTail.Length;
                    if (tailDefaults.CustomTail.Wideness != 0)
                        wideness = tailDefaults.CustomTail.Wideness;
                    if (tailDefaults.CustomTail.Roundness != 0)
                        roundness = tailDefaults.CustomTail.Roundness;
                    // offsetOp.value = tailDefaults.CustomTail.Lift;
                }

                //ONE LAST CHECKPOINT TO CLEAR ANY UNPROVIDED VALUES
                length = (length == 0) ? 4 : length;
                wideness = (wideness == 0) ? 1 : wideness;
                roundness = (roundness == 0) ? 0.1f : roundness;

                //Debug.LogFormat("TAIL VALS: " + length + " - " + wideness + " - " + roundness);
                oldTail = self.tail;
                self.tail = new TailSegment[length];
                for (var i = 0; i < length; i++)
                {
                    float segRad = GetSegmentRadius(i, length, wideness, roundness, 0, pup);
                    self.tail[i] = new TailSegment(self, segRad, (float)((i == 0) ? 4 : 7) * (pup ? 0.5f : 1f), (i > 0) ? self.tail[i - 1] : null, 0.85f, 1f, (i == 0) ? 1f : 0.5f, true);
                    //Debug.LogFormat("RAD: " + segRad);
                }

                //THIS ONE WAS MOON'S, BUT THE AVAILIBLE RANGES AREN'T QUITE RIGHT.
                /*
                oldTail = self.tail; //CARRIED OVER FROM THE NEW VERSION - WW

                //HERE
                int length = customization.CustomTail.EffectiveLength;
                float wideness = customization.CustomTail.EffectiveWideness;
                float roundness = customization.CustomTail.EffectiveRoundness;
                int offset = (int)customization.CustomTail.EffectiveLift;
                var pup = self.player.playerState.isPup;

                //Debug.LogFormat("\n");
                Debug.LogFormat("Length: " + length + " Offset: " + offset);

                offset = (offset > length - 2) ? (length - 2) : offset;  //Detects if the offset is too big compared to the length, and if it is decreases it

                //Debug.Log("New Offset: " + offset);

                //Debug.Log("The condition k should be greater then or equal to is: " + offset);
                //Debug.LogFormat("\n");

                //var doubleLength = 2 * length; WE DON'T USE THIS - WW
                self.tail = new TailSegment[length];
                for (var i = 0; i < length; i++)
                {
                    //WE HAVE A METHOD FOR IT NOW BECAUSE OUR TAIL PREVIEW WANTS TO USE IT
                    float radiusWidth = GetSegmentRadius(i, length, wideness, roundness, offset, pup);
                    self.tail[i] = new TailSegment(self, radiusWidth, (float)((i == 0) ? 4 : 3) * (pup ? 0.5f : 1f), (i > 0) ? self.tail[i - 1] : null, 0.85f, 1f, (i == 0) ? 1f : 0.5f, false);
                }
                */
            }
            //ELSE, JUST USE WHATEVER TAIL SIZE THE GAME GAVE THEM
            else
            {
                oldTail = self.tail;
                for (var i = 0; i < self.tail.Length && i < oldTail.Length; i++)
                {
                    self.tail[i].pos = oldTail[i].pos;
                    self.tail[i].lastPos = oldTail[i].lastPos;
                    self.tail[i].vel = oldTail[i].vel;
                    self.tail[i].terrainContact = oldTail[i].terrainContact;
                    self.tail[i].stretched = oldTail[i].stretched;
                }

                self.tail = playerGraphicsData.originalTail;
            }

            for (var i = 0; i < self.tail.Length && i < oldTail.Length; i++)
            {
                self.tail[i].pos = oldTail[i].pos;
                self.tail[i].lastPos = oldTail[i].lastPos;
                self.tail[i].vel = oldTail[i].vel;
                self.tail[i].terrainContact = oldTail[i].terrainContact;
                self.tail[i].stretched = oldTail[i].stretched;
            }

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();

            playerGraphicsData.tailSegmentRef = self.tail;

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

                                if (sheet.HasAsymmetry(definition.Name))
                                {
                                    playerGraphicsData.LeftSpriteReplacements[specificSprite] = sheet.LeftElements[sprite];
                                    playerGraphicsData.RightSpriteReplacements[specificSprite] = sheet.RightElements[sprite];
                                }

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

            //-FB force reset sprites with only 1 option
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("BodyA");
            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("HipsA");
            sLeaser.sprites[11].element = Futile.atlasManager.GetElementWithName("pixel");

            //if (customization.CustomTail.IsCustom) //-WW ADDING THIS CHECK BECAUSE WHY WOULD WE UPDATE IT IF IT ISN'T CUSTOM?
            ReplaceTailGraphics(self, sLeaser, rCam, false); //WAIT THIS IS FOR THE GRAPHICS NOT THE SIZE...

            self.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            return playerGraphicsData;
        }

        public static void ReplaceTailGraphics(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool uvMapped)
        {
            if (!PlayerGraphicsData.TryGetValue(self, out var playerGraphicsData))
            {
                return;
            }

            if (sLeaser.sprites[2] is TriangleMesh tail)
            {
                playerGraphicsData.tailSegmentRef = self.tail;

                //-WW -ONLY RESIZE THE TAIL IF WE HAVE A CUSTOM TAIL SIZE
                if (Customization.For(self).CustomTail.IsCustom)
                {
                    //sLeaser.sprites[2].RemoveFromContainer();

                    Triangle[] triangles = new Triangle[(self.tail.Length - 1) * 4 + 1];
                    for (int i = 0; i < self.tail.Length - 1; i++)
                    {
                        int num = i * 4;
                        for (int j = 0; j < 4; j++)
                        {
                            triangles[num + j] = new Triangle(num + j, num + j + 1, num + j + 2);
                        }
                    }
                    triangles[(self.tail.Length - 1) * 4] = new Triangle((self.tail.Length - 1) * 4, (self.tail.Length - 1) * 4 + 1, (self.tail.Length - 1) * 4 + 2);
                    
                    //tail = new TriangleMesh("Futile_White", array, tail.customColor, false);
                    //sLeaser.sprites[2] = tail;
                    playerGraphicsData.tailRef = tail;

                    //-- This is so wonky, why can't we just create a new tail? I blame SplitScreenCoop
                    tail.triangles = triangles;
                    int verticeNum = 2;
                    for (int i = 0; i < tail.triangles.Length; i++)
                    {
                        if (tail.triangles[i].a > verticeNum)
                        {
                            verticeNum = tail.triangles[i].a;
                        }
                        if (tail.triangles[i].b > verticeNum)
                        {
                            verticeNum = tail.triangles[i].b;
                        }
                        if (tail.triangles[i].c > verticeNum)
                        {
                            verticeNum = tail.triangles[i].c;
                        }
                    }
                    tail.vertices = new Vector2[verticeNum + 1];
                    tail.UVvertices = new Vector2[verticeNum + 1];
                    for (int j = 0; j < verticeNum; j++)
                    {
                        tail.vertices[j] = new Vector2(0f, 0f);
                        tail.UVvertices[j] = new Vector2(0f, 0f);
                    }
                    if (tail.customColor)
                    {
                        tail.verticeColors = new Color[verticeNum + 1];
                        for (int k = 0; k < tail.verticeColors.Length; k++)
                        {
                            tail.verticeColors[k] = tail._alphaColor;
                        }
                    }
                    tail.Init(FFacetType.Triangle, tail.element, triangles.Length);;

                    //rCam.ReturnFContainer("Midground").AddChild(tail);
                    //tail.MoveBehindOtherNode(sLeaser.sprites[4]);
                }
                //THE REST WE ALWAYS RUN, BECAUSE WE NEED TO APPLY CUSTOM TEXTURES

                if (playerGraphicsData.SpriteReplacements.TryGetValue("TailTexture", out var tailTexture) && tailTexture != null)
                {
                    tail.element = tailTexture;
                }
                else
                {
                    tail.element = playerGraphicsData.originalTailElement;
                }

                if (!uvMapped)
                    MapTailUV(self, playerGraphicsData, tail);
            }
        }

        private static void MapTailUV(PlayerGraphics self, PlayerGraphicsEx playerGraphicsData, TriangleMesh tail)
        {
            const float ASYM_SCALE_FAC = 3.0f; //-FB asymmetric tail is 3 times as wide as normal
            
            float uvYOffset = 0.0f;
            float scaleFac = 1.0f;

            //-FB copy pasted from pearlcat, what could go wrong?
            if (Customization.For(self).CustomTail.IsAsym)
            {
                scaleFac = ASYM_SCALE_FAC;

                Vector2 legsPos = self.legs.pos;
                Vector2 tailPos = self.tail[0].pos;

                // Find the difference between the x positions and convert it into a 0.0 - 1.0 ratio between the two
                float difference = tailPos.x - legsPos.x;


                const float minEffectiveOffset = -15.0f;
                const float maxEffectiveOffset = 15.0f;

                float leftRightRatio = Mathf.InverseLerp(minEffectiveOffset, maxEffectiveOffset, difference);


                // Multiplier determines how many times larger the texture is vertically relative to the displayed portion
                uvYOffset = Mathf.Lerp(0.0f, tail.element.uvTopRight.y - (tail.element.uvTopRight.y / scaleFac), leftRightRatio);
            }

            for (int vertex = tail.vertices.Length - 1; vertex >= 0; vertex--)
            {
                float interpolation = (vertex / 2.0f) / (tail.vertices.Length / 2.0f);
                Vector2 uvInterpolation;

                // Even vertexes
                if (vertex % 2 == 0)
                    uvInterpolation = new Vector2(interpolation, 0.0f);

                // Last vertex
                else if (vertex == tail.vertices.Length - 1)
                    uvInterpolation = new Vector2(1.0f, 0.0f);

                else
                    uvInterpolation = new Vector2(interpolation, 1.0f);

                Vector2 uv;
                uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uvInterpolation.x);
                uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y + uvYOffset, (tail.element.uvTopRight.y / scaleFac) + uvYOffset, uvInterpolation.y);

                tail.UVvertices[vertex] = uv;

                if (tail.verticeColors != null && playerGraphicsData.originalTailColors != null)
                {
                    var colorIndex = vertex % playerGraphicsData.originalTailColors.Length;
                    tail.verticeColors[vertex] = playerGraphicsData.originalTailColors[colorIndex];
                }
            }
        }
    }
}