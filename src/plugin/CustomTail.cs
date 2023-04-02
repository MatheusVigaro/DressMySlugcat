using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DressMySlugcat
{
    [Serializable]
    public class CustomTail
    {
        public float Roundness;
        public float Wideness;
        public float Length;
        public float Lift;
        public string ColorHex;

        public float EffectiveRoundness => (Roundness * 1.4f) + 0.01f;
        public float EffectiveWideness => (Wideness * 10f) + 0.1f;
        public int EffectiveLength => (int)((Length * 15) + 2);
        public float EffectiveLift => Lift;
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

        public bool IsCustom => Roundness > 0 || Wideness > 0 || Length > 0;
    }
}