using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DressMySlugcat.Hooks;
using System.IO;
using Menu;

namespace DressMySlugcat
{

    public static class Utils
    {
        public static Color DefaultColorForSprite(string slugcat, string sprite)
        {
            switch (sprite)
            {
                case "FACE":
                    return DefaultEyeColor(slugcat);
                case "HEAD":
                case "BODY":
                case "ARMS":
                case "HIPS":
                case "LEGS":
                case "TAIL":
                    return DefaultBodyColor(slugcat);
                default:
                    return DefaultExtraColor(slugcat);
            }
        }

        public static Color DefaultBodyColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 0)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[0], out var color) && color != default)
                {
                    return color;
                }
            }

            return Color.white;
        }

        public static Color DefaultEyeColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 1)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[1], out var color) && color != default)
                {
                    return color;
                }
            }

            return new Color(0.062745f, 0.062745f, 0.062745f);
        }

        public static Color DefaultExtraColor(string slugcat)
        {
            var colors = DefaultColors(slugcat);
            if (colors.Count > 2)
            {
                if (ColorUtility.TryParseHtmlString("#" + colors[2], out var color) && color != default)
                {
                    return color;
                }
            }

            return default;
        }

        public static List<string> DefaultColors(string slugcat)
        {
            if (!ExtEnumBase.TryParse(typeof(SlugcatStats.Name), slugcat, true, out var name) || name == null)
            {
                return new();
            }

            return PlayerGraphics.DefaultBodyPartColorHex(name as SlugcatStats.Name);
        }
    }
}