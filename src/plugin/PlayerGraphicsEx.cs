namespace DressMySlugcat;

public class PlayerGraphicsEx
{
    public Dictionary<string, FAtlasElement> SpriteReplacements = [];
    public Dictionary<string, FAtlasElement> LeftSpriteReplacements = [];
    public Dictionary<string, FAtlasElement> RightSpriteReplacements = [];
    public Dictionary<string, Color> SpriteColors = [];
    public string[] SpriteNames;
    public bool IsArtificer;
    public bool IsWatcher;
    public bool IsPupCached;
    public bool ScheduleForRecreation;
    public Customization Customization;
    public TriangleMesh tailRef;
    public TailSegment[] tailSegmentRef;
    public Color tailColor;
    public Color gillEffectColor;


    public TailSegment[] originalTail;
    public FAtlasElement originalTailElement;
    public Color[] originalTailColors;

    public bool TailIntegrity(RoomCamera.SpriteLeaser sLeaser) => sLeaser.sprites.Length > 2 && sLeaser.sprites[2] == tailRef && (sLeaser.drawableObject as PlayerGraphics)?.tail == tailSegmentRef;
}
