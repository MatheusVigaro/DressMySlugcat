using System;
using System.Linq;
using System.Collections.Generic;

namespace DressMySlugcat
{
    [Serializable]
    public class CustomTail
    {
        public float Roundness;
        public float Wideness;
        public float Length;
        public float Lift;

        public float EffectiveRoundness => (Roundness * 1.4f) + 0.01f;
        public float EffectiveWideness =>  (Wideness * 10f) + 0.1f;
        public int EffectiveLength => (int)((Length * 15) + 2);
        public float EffectiveLift => Lift;
        public bool IsCustom => Roundness > 0 || Wideness > 0 || Length > 0 || Lift > 0;
    }
}