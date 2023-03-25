using System;
using System.Linq;
using System.Collections.Generic;

namespace DressMySlugcat
{
    [Serializable]
    public class Customization
    {
        public string Slugcat;
        public string Sprite;
        public string SpriteSheetID;
        public bool Enforce;
        public bool ForceWhiteColor;

        public SpriteSheet SpriteSheet => SpriteSheet.Get(SpriteSheetID);
        public SpriteDefinitions.AvailableSprite SpriteDefinition => SpriteDefinitions.Get(Sprite);
        public FAtlasElement GetElement(string name) => SpriteSheet?.Elements[name];
    }
}