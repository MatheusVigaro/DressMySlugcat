namespace DressMySlugcat.Hooks;

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

        var loc = -1;
        try
        {
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdloc(out loc),
                                                    i => i.MatchLdloc(out _),
                                                    i => i.MatchStfld<FAtlasElement>(nameof(FAtlasElement.name))))
            {
                throw new Exception("Failed to match IL for FAtlas_LoadAtlasData!");
            }
        }
        catch (Exception ex)
        {
            DebugError("Exception when matching IL for FAtlas_LoadAtlasData!");
            DebugError(ex);
            DebugError(il);
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((FAtlasElement element) =>
        {
            if (!string.IsNullOrEmpty(AtlasElementNamePrefix))
            {
                element.name = AtlasElementNamePrefix + element.name;
            }
        });
    }

    public static List<string> Errors = [];
    public static void RegisterError(string friendly, Exception exception = null)
    {
        Errors.Add(friendly);
        DebugWarning("DressMySlugcat: " + friendly + Environment.NewLine + (exception != null ? StackTraceUtility.ExtractStringFromException(exception) : ""));
    }

    public static void ReloadAtlases()
    {
        SpriteSheet.EmptyAtlas = null;
        SpriteSheet.TailAtlas = null;
        foreach (var sheet in SpriteSheets)
        {
            foreach (var atlas in sheet.Atlases)
            {
                Futile.atlasManager.UnloadAtlas(atlas.name);
            }
        }

        SpriteSheets.Clear();

        LoadAtlases();
        SpriteSheet.UpdateDefaults();
    }

    public static void LoadAtlases(string directory = BaseName)
    {
        Errors.Clear();
        LoadAtlasesInternal(directory);
    }

    public static void LoadAtlasesInternal(string directory = BaseName)
    {
        var files = Utils.ListDirectory(directory, includeAll: true).Distinct().ToList();

        var metaFile = files.FirstOrDefault(f => "metadata.json".Equals(Path.GetFileName(f), StringComparison.InvariantCultureIgnoreCase));

        try
        {
            if (!string.IsNullOrEmpty(metaFile))
            {
                if (BaseName.Equals(directory))
                {
                    RegisterError($"Metadata file found in the base directory, please create a subdirectory instead: {metaFile}");
                }

                var spriteSheet = new SpriteSheet();
                try
                {
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
                    if (metaDict.TryGetValue("defaults", out var defaults))
                    {
                        foreach (KeyValuePair<string, object> sprite in defaults as Dictionary<string, object>)
                        {
                            var defaultsDict = sprite.Value as Dictionary<string, object>;
                            foreach (KeyValuePair<string, object> def in defaultsDict)
                            {
                                if ("tail".Equals(sprite.Key, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    switch (def.Key.ToLower())
                                    {
                                        case "color":
                                            if (ColorUtility.TryParseHtmlString((string)def.Value, out var color))
                                            {
                                                spriteSheet.DefaultTail.Color = color;
                                            }
                                            break;
                                        case "length":
                                            if (def.Value is double length)
                                            {
                                                spriteSheet.DefaultTail.Length = (float)length;
                                            }
                                            break;
                                        case "wideness":
                                            if (def.Value is double wideness)
                                            {
                                                spriteSheet.DefaultTail.Wideness = (float)wideness;
                                            }
                                            break;
                                        case "roundness":
                                            if (def.Value is double roundness)
                                            {
                                                spriteSheet.DefaultTail.Roundness = (float)roundness;
                                            }
                                            break;
                                        case "lift":
                                            if (def.Value is double lift)
                                            {
                                                spriteSheet.DefaultTail.Lift = (float)lift;
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (def.Key.ToLower())
                                    {
                                        case "color":
                                            if (ColorUtility.TryParseHtmlString((string)def.Value, out var color))
                                            {
                                                spriteSheet.DefaultColors.Add(sprite.Key.ToUpper(), color);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    RegisterError($"Error loading spritesheet metadata at \"{metaFile}\"!", ex);
                    return;
                }

                if (SpriteSheets.Any(x => x.ID == spriteSheet.ID))
                {
                    RegisterError($"Duplicate spritesheet ID! \"{spriteSheet.ID}\"!");
                    return;
                }

                spriteSheet.Prefix = BaseName + "_" + spriteSheet.ID + "_";

                if (!string.IsNullOrEmpty(spriteSheet.ID) && !string.IsNullOrEmpty(spriteSheet.Name) && !string.IsNullOrEmpty(spriteSheet.Author))
                {
                    foreach (var file in files.Where(f => f.EndsWith(".png")))
                    {
                        var fileNoExt = file.Substring(0, file.Length - 4);
                        if (!files.Any(f => f.EndsWith(fileNoExt + ".txt")))
                        {
                            RegisterError($"Missing txt for spritesheet at \"{file}\"!");
                            continue;
                        }

                        FAtlas atlas = null;
                        try
                        {
                            AtlasElementNamePrefix = spriteSheet.Prefix;
                            atlas = Futile.atlasManager.LoadAtlas(fileNoExt);
                        }
                        catch (Exception ex)
                        {
                            RegisterError($"Error loading spritesheet at \"{file}\"!", ex);
                            return;
                        }
                        finally
                        {
                            AtlasElementNamePrefix = null;
                        }

                        spriteSheet.Atlases.Add(atlas);
                    }

                    SpriteSheets.Add(spriteSheet);

                    try
                    {
                        spriteSheet.ParseAtlases();
                    }
                    catch (Exception ex)
                    {
                        RegisterError($"Error parsing atlases from spritesheet \"{spriteSheet.ID}\"!", ex);
                        return;
                    }
                }
                else
                {
                    RegisterError($"Missing metadata fields in \"{metaFile}\"!");
                    return;
                }
            }
        }

        finally
        {
            var subDirectories = Utils.ListDirectory(directory, true, true).Distinct().ToList();
            foreach (var subDir in subDirectories)
            {
                LoadAtlasesInternal(subDir);
            }
        }
    }
}