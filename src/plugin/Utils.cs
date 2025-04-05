namespace DressMySlugcat
{
    public static class Utils
    {
        public static Color DefaultColorForSprite(string slugcat, string sprite)
        {
            switch (sprite)
            {
                case "FACE":
                    return DefaultEyeColor(slugcat);
                case "HEAD":
                case "BODY":
                case "ARMS":
                case "HIPS":
                case "LEGS":
                case "TAIL":
                case "PIXEL":
                    return DefaultBodyColor(slugcat);
                default:
                    return DefaultExtraColor(slugcat);
            }
        }

        public static Color DefaultBodyColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 0)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[0], out var color) && color != default)
                {
                    return color;
                }
            }

            return Color.white;
        }

        public static Color DefaultEyeColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 1)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[1], out var color) && color != default)
                {
                    return color;
                }
            }

            return new Color(0.062745f, 0.062745f, 0.062745f);
        }

        public static Color DefaultExtraColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 2)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[2], out var color) && color != default)
                {
                    return color;
                }
            }

            return default;
        }

        public static List<string> DefaultColors(string slugcat)
        {
            if (!ExtEnumBase.TryParse(typeof(SlugcatStats.Name), slugcat, true, out var name) || name == null)
            {
                return new();
            }

            return PlayerGraphics.DefaultBodyPartColorHex(name as SlugcatStats.Name);
        }

        public static List<string> ValidSlugcatNames => SlugcatStats.Name.values.entries.Where(x => !SlugcatStats.HiddenOrUnplayableSlugcat(new SlugcatStats.Name(x)) || "Slugpup".Equals(x) || "Inv".Equals(x)).ToList();

        public static string[] ListDirectory(string path, bool directories = false, bool includeAll = false)
        {
            if (Path.IsPathRooted(path))
            {
                return (directories ? Directory.GetDirectories(path) : Directory.GetFiles(path));
            }

            if (!Plugin.Options?.LoadInactiveMods?.Value ?? false)
            {
                return AssetManager.ListDirectory(path, directories, includeAll);
            }
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            list3.Add(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"));
            for (int i = 0; i < ModManager.InstalledMods.Count; i++)
            {
                list3.Add(ModManager.InstalledMods[i].path);
            }

            list3.Add(Custom.RootFolderDirectory());
            foreach (string item in list3)
            {
                string path2 = Path.Combine(item, path.ToLowerInvariant());
                if (!Directory.Exists(path2))
                {
                    continue;
                }

                string[] array = (directories ? Directory.GetDirectories(path2) : Directory.GetFiles(path2));
                for (int j = 0; j < array.Length; j++)
                {
                    string text = array[j].ToLowerInvariant();
                    string fileName = Path.GetFileName(text);
                    if (!list2.Contains(fileName) || includeAll)
                    {
                        list.Add(text);
                        if (!includeAll)
                        {
                            list2.Add(fileName);
                        }
                    }
                }
            }

            return list.ToArray();
        }
    }
}