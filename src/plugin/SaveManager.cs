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
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections;
using static DressMySlugcat.SaveManager;

namespace DressMySlugcat
{
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

            foreach (var name in SlugcatStats.Name.values.entries.Where(x => !x.StartsWith("JollyPlayer")).ToList())
            {
                for (var i = 0; i < 4; i++)
                {
                    if (!Customizations.Any(x => x.Slugcat == name && x.PlayerNumber == i))
                    {
                        Customizations.Add(new() { Slugcat = name, PlayerNumber = i });
                    }
                }
            }

            foreach (var customization in Customizations)
            {
                customization.CustomTail ??= new();
            }

            Customization.CleanDefaults();
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
}