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

namespace DressMySlugcat
{
    public static class SaveManager
    {
        public static string root = Application.persistentDataPath + Path.DirectorySeparatorChar + Plugin.BaseName + Path.DirectorySeparatorChar;
        public static string spriteReplacementsFile = root + "spritereplacements.dat";
        public static List<SpriteReplacement> SpriteReplacements = new();

        public static void Load()
        {
            if (!Directory.Exists(root) || !File.Exists(spriteReplacementsFile))
            {
                Save();
            }

            SpriteReplacements.Clear();
            SaveData data;

            FileStream fs = new FileStream(spriteReplacementsFile, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                data = (SaveData) formatter.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }

            if (data?.SpriteReplacements != null)
            {
                SpriteReplacements = data.SpriteReplacements;
            }
        }

        public static void Save()
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            SaveData data = new();
            data.SpriteReplacements = SpriteReplacements;

            FileStream fs = new FileStream(spriteReplacementsFile, FileMode.Create);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
            }
            finally
            {
                fs.Close();
            }
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
    }
}