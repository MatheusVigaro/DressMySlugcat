using System.Collections.Generic;
using UnityEngine;

namespace DressMySlugcat
{
    public class PlayerGraphicsEx
    {
        public Dictionary<string, FAtlasElement> SpriteReplacements = new();
        public Dictionary<string, Color> SpriteColors = new();
        public string[] SpriteNames;
        public bool IsArtificer;
        public Customization Customization;
        public TriangleMesh tailRef;
        public Color tailColor;
        public Color gillEffectColor;

        public bool TailIntegrity(RoomCamera.SpriteLeaser sLeaser) => sLeaser.sprites.Length > 2 && sLeaser.sprites[2] == tailRef;
    }
}