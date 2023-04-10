using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace DressMySlugcat
{
    [Serializable]
    public class Customization
    {
        public CustomTail CustomTail = new();
        public List<CustomSprite> CustomSprites = new();

        public string Slugcat;
        public int PlayerNumber;

        public CustomSprite CustomSprite(string spriteName, bool createIfNotExists = false)
        {
            var customSprite = CustomSprites.Where(x => x.Sprite == spriteName).FirstOrDefault();
            if (createIfNotExists && customSprite == null)
            {
                customSprite = new()
                {
                    Sprite = spriteName,
                    Enforce = true,
                    SpriteSheetID = SpriteSheet.DefaultName
                };

                CustomSprites.Add(customSprite);
            }

            return customSprite;
        }

        public Customization Copy()
        {
            var formatter = new BinaryFormatter();

            using (var ms = new MemoryStream()) {
                formatter.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin);
                return (Customization)formatter.Deserialize(ms);
            }
        }

        public bool Matches(string slugcatName, int playerNumber = 0) => slugcatName == Slugcat && (playerNumber == PlayerNumber);
        public bool Matches(Player player) => Matches(player.slugcatStats.name.value, player.playerState.playerNumber);
        public bool Matches(PlayerGraphics playerGraphics) => Matches(playerGraphics.player);

        public static Customization For(string slugcatName, int playerNumber = 0, bool mergeDefaults = true)
        {
            var customization = SaveManager.Customizations.FirstOrDefault(x => x.Matches(slugcatName, playerNumber));
            if (!mergeDefaults)
            {
                return customization;
            }

            var defaults = SpriteDefinitions.GetSlugcatDefault(customization.Slugcat, customization.PlayerNumber)?.Copy();

            if (defaults == null)
            {
                return customization;
            }

            customization = customization.Copy();

            if (!customization.CustomTail.IsCustom)
            {
                customization.CustomTail.Length = defaults.CustomTail.Length;
                customization.CustomTail.Wideness = defaults.CustomTail.Wideness;
                customization.CustomTail.Roundness = defaults.CustomTail.Roundness;
                customization.CustomTail.Lift = defaults.CustomTail.Lift;
            }

            if (customization.CustomTail.Color == default)
            {
                customization.CustomTail.Color = defaults.CustomTail.Color;
            }

            foreach (var sprite in defaults.CustomSprites)
            {
                if (!customization.CustomSprites.Any(x => x.Sprite == sprite.Sprite))
                {
                    customization.CustomSprites.Add(sprite);
                }
            }

            return customization;
        }

        public static Customization For(Player player, bool mergeDefaults = true) => For(player.slugcatStats.name.value, player.playerState.playerNumber, mergeDefaults);
        public static Customization For(PlayerGraphics playerGraphics, bool mergeDefaults = true) => For(playerGraphics.player, mergeDefaults);

        public static void CleanDefaults()
        {
            foreach (var customization in SaveManager.Customizations)
            {
                foreach (var sprite in customization.CustomSprites.ToList())
                {
                    if (sprite.SpriteSheetID == SpriteSheet.DefaultName && (sprite.Color == default || sprite.Color == Utils.DefaultColorForSprite(customization.Slugcat, sprite.Sprite)))
                    {
                        customization.CustomSprites.Remove(sprite);
                    }
                }
            }
        }

        #region Deprecated
        [Obsolete("Use Customization.CustomSprites instead")]
        public string Sprite;
        [Obsolete("Use Customization.CustomSprites instead")]
        public string SpriteSheetID;
        [Obsolete("Use Customization.CustomSprites instead")]
        public bool Enforce;
        [Obsolete("Use Customization.CustomSprites instead")]
        public bool ForceWhiteColor;

        [Obsolete("Use Customization.CustomSprites instead")]
        public SpriteSheet SpriteSheet => SpriteSheet.Get(SpriteSheetID);

        [Obsolete("Use Customization.CustomSprites instead")]
        public SpriteDefinitions.AvailableSprite SpriteDefinition => SpriteDefinitions.Get(Sprite);

        [Obsolete("Use Customization.CustomSprites instead")]
        public FAtlasElement GetElement(string name) => SpriteSheet?.Elements[name];
        #endregion
    }
}