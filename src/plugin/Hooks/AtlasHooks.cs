using HUD;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace DressMySlugcat.Hooks
{
    public class AtlasHooks
    {
        public static void Init()
        {
            IL.FAtlas.LoadAtlasData += FAtlas_LoadAtlasData;
        }

        public static string AtlasElementNamePrefix;

        private static void FAtlas_LoadAtlasData(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdloc(7),
                                                    i => i.MatchLdloc(8),
                                                    i => i.MatchStfld<FAtlasElement>("name")))
            {
                return;
            }

            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldloc, 7);
            cursor.EmitDelegate((FAtlasElement element) =>
            {
                if (!string.IsNullOrEmpty(AtlasElementNamePrefix))
                {
                    element.name = AtlasElementNamePrefix + element.name;
                }
            });
        }

        public static void LoadAtlases(string directory = Plugin.BaseName)
        {
            var files = AssetManager.ListDirectory(directory, includeAll: true).Distinct().ToList();

            var metaFile = files.FirstOrDefault(f => "metadata.json".Equals(Path.GetFileName(f), StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrEmpty(metaFile))
            {
                var spriteSheet = new SpriteSheet();
                var metaDict = File.ReadAllText(metaFile).dictionaryFromJson();
                if (metaDict.TryGetValue("id", out var id))
                {
                    spriteSheet.ID = (string)id;
                }
                if (metaDict.TryGetValue("name", out var name))
                {
                    spriteSheet.Name = (string)name;
                }
                if (metaDict.TryGetValue("author", out var author))
                {
                    spriteSheet.Author = (string)author;
                }
                spriteSheet.Prefix = Plugin.BaseName + "_" + spriteSheet.ID + "_";


                if (!string.IsNullOrEmpty(spriteSheet.ID))
                {
                    foreach (var file in files.Where(f => f.EndsWith(".png")))
                    {
                        var fileNoExt = file.Substring(0, file.Length - 4);
                        if (!files.Any(f => f.EndsWith(fileNoExt + ".txt")))
                        {
                            continue;
                        }

                        FAtlas atlas = null;
                        try
                        {
                            AtlasElementNamePrefix = spriteSheet.Prefix;
                            atlas = Futile.atlasManager.LoadAtlas(fileNoExt);
                        }
                        finally
                        {
                            AtlasElementNamePrefix = null;
                        }

                        spriteSheet.Atlases.Add(atlas);
                    }
                }

                spriteSheet.ParseAtlases();
                Plugin.SpriteSheets.Add(spriteSheet);
            }

            var subDirectories = AssetManager.ListDirectory(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadAtlases(subDir);
            }
        }
    }
}