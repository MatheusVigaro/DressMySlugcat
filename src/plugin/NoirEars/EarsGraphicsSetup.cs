namespace DressMySlugcat.NoirEars;

public static partial class NoirEars
{
    public static void ApplyHooks()
    {
        On.PlayerGraphics.ctor += PlayerGraphicsOnctor;
        On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphicsOnApplyPalette;
        On.PlayerGraphics.Reset += PlayerGraphicsOnReset;
        On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
    }

    public static void LoadAtlases()
    {
        var earsAtlas = Futile.atlasManager.LoadAtlas("atlases/Ears");
    }

    #region Consts
    private const int BodySpr = 0; //Midground
    private const int HipsSpr = 1;
    private const int TailSpr = 2;
    private const int HeadSpr = 3;
    private const int LegsSpr = 4;
    private const int ArmSpr = 5;
    private const int ArmSpr2 = 6; //Midground
    private const int OTOTArmSpr = 7; //Foreground
    private const int OTOTArmSpr2 = 8; //Foreground
    private const int FaceSpr = 9; //Midground

    const string Ears = "Ears";
    #endregion

    private static Vector2 EarAttachPos(EarsData earsData, int earNum, float timestacker)
    {
        var graphics = (PlayerGraphics)earsData.Cat.graphicsModule;
        var numXMod = earNum == 0 ? -4 : 4; //TODO make the numbers customizable (X and Y position offset)
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(numXMod, 1.5f), graphics.head.pos + new Vector2(numXMod, 1.5f), timestacker) + Vector3.Slerp(earsData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }
}