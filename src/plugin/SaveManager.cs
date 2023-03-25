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
        public static string spriteReplacementsFile = root + "spritereplacements.dat";
        public static string customizationsFile = root + "customizations.dat";
        public static List<Customization> Customizations = new();

        public static void Load()
        {
            if (File.Exists(spriteReplacementsFile))
            {
                MigrateOldSave();
            }

            if (!Directory.Exists(root) || !File.Exists(customizationsFile))
            {
                Save();
            }

            using (var fs = new FileStream(customizationsFile, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                Customizations = (List<Customization>)formatter.Deserialize(fs);
            }
        }

        public static void Save()
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            using (var fs = new FileStream(customizationsFile, FileMode.Create))
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