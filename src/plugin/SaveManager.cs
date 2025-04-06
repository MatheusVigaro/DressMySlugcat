namespace DressMySlugcat;

public static class SaveManager
{
    public static string root = Application.persistentDataPath + Path.DirectorySeparatorChar + Plugin.BaseName + Path.DirectorySeparatorChar;
    [ObsoleteAttribute("Use customizationFile instead")]
    public static string spriteReplacementsFile = root + "spritereplacements.dat";
    [ObsoleteAttribute("Use customizationFile instead")]
    public static string oldCustomizationsFile = root + "customizations.dat";
    public static string customizationFile = root + "customization.dat";
    public static List<Customization> Customizations = new();

    public static void Load()
    {
        if (File.Exists(spriteReplacementsFile))
        {
            MigrateOldSave();
        }

        if (File.Exists(oldCustomizationsFile))
        {
            MigrateOldSave2();
        }

        if (!Directory.Exists(root) || !File.Exists(customizationFile))
        {
            Save();
        }

        using (var fs = new FileStream(customizationFile, FileMode.Open))
        {
            var formatter = new BinaryFormatter();
            Customizations = (List<Customization>)formatter.Deserialize(fs);
        }

        InitSlugcatCustomizations();

        foreach (var customization in Customizations)
        {
            customization.CustomTail ??= new();
        }

        Customization.CleanDefaults();
    }

    public static void InitSlugcatCustomizations()
    {
        foreach (var name in Utils.ValidSlugcatNames)
        {
            //for (var i = 0; i < 4; i++)
            for (var i = 0; i < Custom.rainWorld.options.controls.Length; i++) //-WW -DYNAMIC LENGTH
            {
                if (!Customizations.Any(x => x.Slugcat == name && x.PlayerNumber == i))
                {
                    Customizations.Add(new() { Slugcat = name, PlayerNumber = i });
                }
            }
        }
    }

    public static void Save()
    {
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        using (var fs = new FileStream(customizationFile, FileMode.Create))
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(fs, Customizations);
        }
    }

    #region Legacy save
    public static void MigrateOldSave()
    {
        SaveData data;
        using (var fs = new FileStream(spriteReplacementsFile, FileMode.Open))
        {
            var formatter = new BinaryFormatter();

            data = (SaveData)formatter.Deserialize(fs);
        }

        if (data?.SpriteReplacements != null)
        {
            Customizations.Clear();

            foreach (var replacement in data.SpriteReplacements)
            {
                Customizations.Add(new()
                {
                    Slugcat = replacement.slugcat,
                    Sprite = replacement.sprite,
                    SpriteSheetID = replacement.replacement
                });
            }
        }

        Save();
        File.Delete(spriteReplacementsFile);
    }

    public static void MigrateOldSave2()
    {
        using (var fs = new FileStream(oldCustomizationsFile, FileMode.Open))
        {
            var formatter = new BinaryFormatter();
            var customizations = (List<Customization>)formatter.Deserialize(fs);

            foreach (var oldCustomization in customizations)
            {
                var customization = Customizations.FirstOrDefault(x => x.Slugcat == oldCustomization.Slugcat);
                if (customization == null)
                {
                    customization = new() { Slugcat = oldCustomization.Slugcat };
                    Customizations.Add(customization);
                }
                customization.CustomSprites.Add(new()
                {
                    Sprite = oldCustomization.Sprite,
                    SpriteSheetID = oldCustomization.SpriteSheetID,
                    Enforce = oldCustomization.Enforce,
                    ForceWhiteColor = oldCustomization.ForceWhiteColor
                });
            }
        }

        Save();
        File.Delete(oldCustomizationsFile);
    }

    [Serializable]
    public class SaveData
    {
        public List<SpriteReplacement> SpriteReplacements;
    }

    [Serializable]
    public class SpriteReplacement
    {
        public string slugcat;
        public string sprite;
        public string replacement;
        public bool enforce;
    }
    #endregion
}