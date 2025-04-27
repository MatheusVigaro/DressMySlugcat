namespace DressMySlugcat;

[Serializable]
public class CustomSprite
{
    public string Sprite;
    public string SpriteSheetID;
    public bool Enforce;
    public string ColorHex;

    public Color Color
    {
        get
        {
            if (!ColorUtility.TryParseHtmlString(ColorHex, out var color) || (color.r == 0 && color.g == 0 && color.b == 0))
            {
                return default;
            }
            return color;
        }
        set
        {
            ColorHex = "#" + ColorUtility.ToHtmlStringRGB(value);
        }
    }

    public SpriteSheet SpriteSheet => SpriteSheet.Get(SpriteSheetID);
    public SpriteDefinitions.AvailableSprite SpriteDefinition => SpriteDefinitions.Get(Sprite);
    public FAtlasElement GetElement(string name) => SpriteSheet?.Elements[name];

    #region Deprecated
    [Obsolete]
    public bool ForceWhiteColor;
    #endregion
}