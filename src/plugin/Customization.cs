using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DressMySlugcat
{
    [Serializable]
    public class Customization
    {
        public CustomTail CustomTail = new();
        public List<CustomSprite> CustomSprites = new();

        public string Slugcat;
        public int PlayerNumber;

        public CustomSprite CustomSprite(string spriteName) => CustomSprites.Where(x => x.Sprite == spriteName).FirstOrDefault();

        public bool Matches(string slugcatName, int playerNumber = 0) => slugcatName == Slugcat && (playerNumber == PlayerNumber);
        public bool Matches(Player player) => Matches(player.slugcatStats.name.value, player.playerState.playerNumber);
        public bool Matches(PlayerGraphics playerGraphics) => Matches(playerGraphics.player);

        public static Customization For(string slugcatName, int playerNumber = 0) => SaveManager.Customizations.FirstOrDefault(x => x.Matches(slugcatName, playerNumber));
        public static Customization For(Player player) => For(player.slugcatStats.name.value, player.playerState.playerNumber);
        public static Customization For(PlayerGraphics playerGraphics) => For(playerGraphics.player);

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