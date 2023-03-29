using System.Collections.Generic;

namespace DressMySlugcat
{
    public class PlayerGraphicsEx
    {
        public Dictionary<string, FAtlasElement> SpriteReplacements = new();
        public string[] SpriteNames;
        public bool IsArtificer;
        public Customization Customization;
        public TriangleMesh tailRef;

        public bool TailIntegrity(RoomCamera.SpriteLeaser sLeaser) => sLeaser.sprites.Length > 2 && sLeaser.sprites[2] == tailRef;
    }
}