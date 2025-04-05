using DressMySlugcat.Hooks;

namespace DressMySlugcat
{
    public class PlayerGraphicsDummy
    {
        public FSprite[] Sprites;
        public FSprite[] TailSprites; //-WW
        public FContainer Container;
        public static readonly float tailXScale = 0.15f;
        public Vector2 SlugcatPosition
        {
            get
            {
                return Sprites[0].GetPosition();
            }
            set
            {
                Sprites[0].SetPosition(value);
                UpdateSpritePositions();
            }
        }

        FancyMenu owner;

        public PlayerGraphicsDummy(FancyMenu owner)
        {
            this.owner = owner;

            Container = new FContainer();
            Futile.stage.AddChild(Container);

            Sprites = new FSprite[11];
            Sprites[0] = new FSprite("BodyA");
            Sprites[1] = new FSprite("HipsA");
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[13]
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(4, 5, 6),
                new TriangleMesh.Triangle(5, 6, 7),
                new TriangleMesh.Triangle(8, 9, 10),
                new TriangleMesh.Triangle(9, 10, 11),
                new TriangleMesh.Triangle(12, 13, 14),
                new TriangleMesh.Triangle(2, 3, 4),
                new TriangleMesh.Triangle(3, 4, 5),
                new TriangleMesh.Triangle(6, 7, 8),
                new TriangleMesh.Triangle(7, 8, 9),
                new TriangleMesh.Triangle(10, 11, 12),
                new TriangleMesh.Triangle(11, 12, 13)
            };
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
            Sprites[2] = triangleMesh;
            Sprites[3] = new FSprite("HeadA0");
            Sprites[4] = new FSprite("LegsA0");
            Sprites[5] = new FSprite("PlayerArm12");
            Sprites[6] = new FSprite("PlayerArm12");
            Sprites[7] = new FSprite("OnTopOfTerrainHand");
            Sprites[8] = new FSprite("OnTopOfTerrainHand");
            Sprites[8].scaleX = -1f;
            Sprites[9] = new FSprite("FaceA0");

            Sprites[9].color = new Color(0.1f, 0.1f, 0.1f);

            Sprites[10] = new FSprite("pixel");

            //-WW -AN ATTEMPT AT TAIL SPRITES
            TailSprites = new FSprite[15];
            for (var i = 0; i < TailSprites.Length; i++)
            {
                TailSprites[i] = new FSprite("Futile_White");
            }

            UpdateSprites();

            AddToContainer();
        }

        public void AddToContainer()
        {
            foreach (var sprite in TailSprites) // -WW 
            {
                Container.AddChild(sprite);
            }

            foreach (var sprite in Sprites)
            {
                Container.AddChild(sprite);
            }

            //-- Forcing arms behind body
            Sprites[5].MoveBehindOtherNode(Sprites[0]);
            Sprites[6].MoveBehindOtherNode(Sprites[0]);

            //-- Forcing legs behind hips, make this an optional toggle and implement ingame
            Sprites[4].MoveBehindOtherNode(Sprites[1]);
        }

        public void UpdateSpritePositions()
        {
            var baseSprite = Sprites[0];

            Sprites[0].x = 0 + baseSprite.x;
            Sprites[0].y = 0 + baseSprite.y;
            Sprites[0].anchorX = 0.5f;
            Sprites[0].anchorY = 0.7894737f;
            Sprites[0].scaleX = 0.9551261f;
            Sprites[0].scaleY = 1;
            Sprites[0].rotation = 4.872149f;
            Sprites[1].x = -1.000122f + baseSprite.x;
            Sprites[1].y = -11.73291f + baseSprite.y;
            Sprites[1].anchorX = 0.5f;
            Sprites[1].anchorY = 0.5f;
            Sprites[1].scaleX = 1.002563f;
            Sprites[1].scaleY = 1;
            Sprites[1].rotation = -175.3343f;
            Sprites[2].isVisible = false;
            Sprites[3].x = -0.04528809f + baseSprite.x;
            Sprites[3].y = 2.469299f + baseSprite.y;
            Sprites[3].anchorX = 0.5f;
            Sprites[3].anchorY = 0.5f;
            Sprites[3].scaleX = 1;
            Sprites[3].scaleY = 1;
            Sprites[3].rotation = 3.578826f;
            Sprites[4].x = -0.0001831055f + baseSprite.x;
            Sprites[4].y = -23.5994f + baseSprite.y;
            Sprites[4].anchorX = 0.5f;
            Sprites[4].anchorY = 0.25f;
            Sprites[4].scaleX = 1;
            Sprites[4].scaleY = 1;
            Sprites[4].rotation = 0;
            Sprites[5].x = -21f + baseSprite.x;
            Sprites[5].y = -2.017578f + baseSprite.y;
            Sprites[5].anchorX = 0.9f;
            Sprites[5].anchorY = 0.5f;
            Sprites[5].scaleX = 1;
            Sprites[5].scaleY = -1;
            Sprites[5].rotation = 180f;
            Sprites[6].x = 21f + baseSprite.x;
            Sprites[6].y = -2 + baseSprite.y;
            Sprites[6].anchorX = 0.9f;
            Sprites[6].anchorY = 0.5f;
            Sprites[6].scaleX = 1;
            Sprites[6].scaleY = 1;
            Sprites[6].rotation = 0f;
            Sprites[7].isVisible = false;
            Sprites[8].isVisible = false;
            Sprites[9].x = -0.04528809f + baseSprite.x;
            Sprites[9].y = 0.4692993f + baseSprite.y;
            Sprites[9].anchorX = 0.5f;
            Sprites[9].anchorY = 0.5f;
            Sprites[9].scaleX = 1;
            Sprites[9].scaleY = 1;
            Sprites[9].rotation = 0;
            Sprites[10].x = baseSprite.x;
            Sprites[10].y = 16.0f + baseSprite.y;
            Sprites[10].anchorX = 0.5f;
            Sprites[10].anchorY = 0.5f;
            Sprites[10].rotation = 0;

            for (var i = 0; i < TailSprites.Length; i++)
            {
                TailSprites[i].x = -1.000122f + baseSprite.x;
                TailSprites[i].y = (-17.0f - ((i) * 8)) + baseSprite.y;
                TailSprites[i].anchorX = 0.5f;
                TailSprites[i].anchorY = 0.5f;
                //TailSprites[i].scaleX = 0.3f;
                TailSprites[i].scaleY = 0.3f;
                TailSprites[i].rotation = 0;
                //TailSprites[i].alpha = 0.5f;
            }
            
        }

        internal void UpdateSprites()
        {
            var customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex);

            var customSprite = customization.CustomSprite("BODY");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("BodyA"))
            {
                Sprites[0].element = customSprite.SpriteSheet.Elements["BodyA"];
            }
            else
            {
                Sprites[0].element = Futile.atlasManager.GetElementWithName("BodyA");
            }
            Sprites[0].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "BODY");

            customSprite = customization.CustomSprite("HIPS");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("HipsA"))
            {
                Sprites[1].element = customSprite.SpriteSheet.Elements["HipsA"];
            }
            else
            {
                Sprites[1].element = Futile.atlasManager.GetElementWithName("HipsA");
            }
            Sprites[1].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "HIPS");

            customSprite = customization.CustomSprite("HEAD");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("HeadA0"))
            {
                Sprites[3].element = customSprite.SpriteSheet.Elements["HeadA0"];
            }
            else
            {
                Sprites[3].element = Futile.atlasManager.GetElementWithName("HeadA0");
            }
            Sprites[3].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "HEAD");

            customSprite = customization.CustomSprite("LEGS");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("LegsA0"))
            {
                Sprites[4].element = customSprite.SpriteSheet.Elements["LegsA0"];
            }
            else
            {
                Sprites[4].element = Futile.atlasManager.GetElementWithName("LegsA0");
            }
            Sprites[4].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "LEGS");

            customSprite = customization.CustomSprite("ARMS");
            bool defaultArms = true;
            if (customSprite?.SpriteSheet != null)
            {
                var sheet = customSprite.SpriteSheet;
                //-WW CHECK IF ANY ARM SPRITES ARE THERE, OR WE NEED TO RUN DEFAULT AS BACKUP
                if ((sheet.HasAsymmetry("ARMS") && sheet.LeftElements.ContainsKey("PlayerArm12")) || sheet.Elements.ContainsKey("PlayerArm12"))
                    defaultArms = false;
            }

            if (!defaultArms)
            {
                var sheet = customSprite.SpriteSheet;
                if (sheet.HasAsymmetry("ARMS"))
                {
                    Sprites[5].element = sheet.LeftElements["PlayerArm12"];
                    Sprites[6].element = sheet.RightElements["PlayerArm12"];
                    Sprites[7].element = sheet.LeftElements["OnTopOfTerrainHand"];
                    Sprites[8].element = sheet.RightElements["OnTopOfTerrainHand"];
                }
                else
                {
                    Sprites[5].element = sheet.Elements["PlayerArm12"];
                    Sprites[6].element = sheet.Elements["PlayerArm12"];
                    Sprites[7].element = sheet.Elements["OnTopOfTerrainHand"];
                    Sprites[8].element = sheet.Elements["OnTopOfTerrainHand"];
                }
            }
            else
            {
                Sprites[5].element = Futile.atlasManager.GetElementWithName("PlayerArm12");
                Sprites[6].element = Futile.atlasManager.GetElementWithName("PlayerArm12");
                Sprites[7].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
                Sprites[8].element = Futile.atlasManager.GetElementWithName("OnTopOfTerrainHand");
            }
            Sprites[5].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "ARMS");
            Sprites[6].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "ARMS");
            Sprites[7].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "ARMS");
            Sprites[8].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "ARMS");

            customSprite = customization.CustomSprite("FACE");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("FaceA0"))
            {
                Sprites[9].element = customSprite.SpriteSheet.Elements["FaceA0"];
            }
            else
            {
                Sprites[9].element = Futile.atlasManager.GetElementWithName("FaceA0");
            }
            Sprites[9].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "FACE");


            //-FB this was missing for the mark, caused problems switching in game
            customSprite = customization.CustomSprite("PIXEL");
            if (customSprite?.SpriteSheet != null && customSprite.SpriteSheet.Elements.ContainsKey("pixel"))
            {
                Sprites[10].element = customSprite.SpriteSheet.Elements["pixel"];
            }
            else
            {
                Sprites[10].element = Futile.atlasManager.GetElementWithName("pixel");
            }
            Sprites[10].color = customSprite?.Color != default && customSprite?.Color.a != 0 ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, "PIXEL");

            //-FB this hack kinda sucks but whatever
            if (Sprites[10].element.sourcePixelSize == Vector2.one)
            {
                Sprites[10].scaleX = 3.5f;
                Sprites[10].scaleY = 3.5f;
            }
            else
            {
                Sprites[10].scaleX = 0.6f;
                Sprites[10].scaleY = 0.6f;
            }


            var tailColor = customization.CustomTail.Color;

            //-WW
            for (var i = 0; i < TailSprites.Length; i++)
            {
                TailSprites[i].color = tailColor != default && tailColor.a != 0 ? tailColor : Utils.DefaultColorForSprite(owner.selectedSlugcat, "HIPS");
                TailSprites[i].alpha = 0.5f;
                bool pup = false;
                float radX = PlayerGraphicsHooks.GetSegmentRadius(i, (int)customization.CustomTail.Length, customization.CustomTail.Wideness, customization.CustomTail.Roundness, (int)customization.CustomTail.Lift, pup);
                TailSprites[i].scaleX = tailXScale * radX;
                //Debug.LogFormat("UPDATE VISIBLE SEGMENTS. ");
                if (i <= (int)customization.CustomTail.Length -1)
                    TailSprites[i].isVisible = true;
                else
                    TailSprites[i].isVisible = false;
            }
            if (customization.CustomTail.CustTailShape == false)
            {
                DefaultDummyTailDisplay();
            }
        }

        static readonly float[] defaultTailChart = { 5.8f, 4f, 2.5f, 1f};

        public void DefaultDummyTailDisplay()
        {
            //HIDE THE WHOLE TAIL FIRST
            for (var i = 0; i < TailSprites.Length; i++)
            {
                TailSprites[i].isVisible = false;
            }

            //-WW CHECK IF WE'RE A CUSTOM SLUGCAT WITH CUSTOM TAIL DEFAULT DEFINITIONS
            Customization customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false);
            var defaults = SpriteDefinitions.GetSlugcatDefault(customization.Slugcat, customization.PlayerNumber)?.Copy();
            if (defaults != null)
            {
                int length = (int)defaults.CustomTail.Length;
                float wideness = defaults.CustomTail.Wideness;
                float roundness = defaults.CustomTail.Roundness;
                //float lift = defaults.CustomTail.Lift;

                if (length == 0)
                {
                    //Debug.Log("LENGTH = 0");
                    length = 4;
                }

                if (wideness == 0)
                    wideness = 1;
                if (roundness == 0)
                    roundness = 0.1f;


                var pup = false;

                for (var i = 0; i < length; i++)
                {
                    float segRad = PlayerGraphicsHooks.GetSegmentRadius(i, length, wideness, roundness, 0, pup);
                    TailSprites[i].scaleX = tailXScale * segRad;
                    TailSprites[i].isVisible = true;
                }
                return;
            }
            else
            {   //BASE-GAME SLUGCAT. TAKE THE TRUE, HARDCODED TAIL SIZE DEFINITION
                for (var i = 0; i < 4; i++)
                {
                    float rad = defaultTailChart[i];
                    TailSprites[i].isVisible = true;
                    TailSprites[i].scaleX = tailXScale * rad;
                }
            }
        }
    }
}