namespace DressMySlugcat;

[Serializable]
public class CustomTail
{
    public float Roundness;
    public float Wideness;
    public float Length;
    public float Lift;
    public string ColorHex;
    public bool CustTailShape; //-WW
    public bool AsymTail;
    public bool ForbidTailResize;

    //public float EffectiveRoundness => (Roundness * 1.4f) + 0.01f;
    //public float EffectiveWideness => (Wideness * 10f) + 0.1f;
    //public int EffectiveLength => (int)((Length * 15) + 2);
    //THESE ARE SET IN THE SLIDER NOW, LIKE A SANE PERSON -WW
    public float EffectiveRoundness => Roundness;
    public float EffectiveWideness => Wideness;
    public int EffectiveLength => (int)Length;
    public float EffectiveLift => Lift;
    public bool EffectiveCustTailShape => CustTailShape; //-WW
    public Color Color
    {
        get
        {
            if (!ColorUtility.TryParseHtmlString(ColorHex, out var color) || color.r == 0 && color.g == 0 && color.b == 0)
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

    //public bool IsCustom => Roundness > 0 || Wideness > 0 || Length > 0;
    public bool IsCustom => EffectiveCustTailShape;

    public bool IsAsym => AsymTail; //-FB i'm not sure why IsCustom has that many steps but i won't question it
}