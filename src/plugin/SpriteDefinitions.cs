namespace DressMySlugcat;

public static class SpriteDefinitions
{
    public static List<AvailableSprite> AvailableSprites = new();
    public static List<Customization> SlugcatDefaults = new();

    public static AvailableSprite Get(string name) => AvailableSprites.FirstOrDefault(x => x.Name == name);

    public static bool IsInit;

    public static void AddSprite(AvailableSprite sprite)
    {
        if (AvailableSprites.Any(x => x.Name == sprite.Name))
        {
            return;
        }

        AvailableSprites.Add(sprite);
        SpriteSheet.UpdateDefaults();
    }

    public static void AddSlugcatDefault(Customization customization, string spriteName)
    {
        SlugcatDefaults.Add(customization);
    }

    //public static Customization GetSlugcatDefault(string slugcat, int playerNumber) => SlugcatDefaults.Where(x => x.Slugcat == slugcat && x.PlayerNumber == playerNumber).LastOrDefault();
    public static Customization GetSlugcatDefault(string slugcat, int playerNumber)
    {
        if (playerNumber > 3) //WW- So Myriad players past 4 can use defaults
            playerNumber = 0;

        return (from x in SpriteDefinitions.SlugcatDefaults
                where x.Slugcat == slugcat && x.PlayerNumber == playerNumber
                select x).LastOrDefault<Customization>();
    }

    public static void Init()
    {
        if (IsInit) return;
        IsInit = true;

        var head = new AvailableSprite()
        {
            Name = "HEAD",
            Description = "Head",
            GallerySprite = "HeadA0",
            RequiredSprites = new() { "HeadA0", "HeadA1", "HeadA2", "HeadA3", "HeadA4", "HeadA5", "HeadA6", "HeadA7", "HeadA8", "HeadA9", "HeadA10", "HeadA11", "HeadA12", "HeadA13", "HeadA14", "HeadA15", "HeadA16", "HeadA17" }
        };

        for (int i = 0; i <= 17; i++)
        {
            head.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Saint",
                GenericName = "HeadA" + i,
                SpecificName = "HeadB" + i
            });

            head.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Slugpup",
                GenericName = "HeadA" + i,
                SpecificName = "HeadC" + i
            });
        }

        AvailableSprites.Add(head);

        var face = new AvailableSprite()
        {
            Name = "FACE",
            Description = "Face",
            GallerySprite = "FaceA0",
            RequiredSprites = new() { "FaceA0", "FaceA1", "FaceA2", "FaceA3", "FaceA4", "FaceA5", "FaceA6", "FaceA7", "FaceA8", "FaceB0", "FaceB1", "FaceB2", "FaceB3", "FaceB4", "FaceB5", "FaceB6", "FaceB7", "FaceB8", "FaceDead", "FaceStunned" }
        };

        for (int i = 0; i <= 8; i++)
        {
            face.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Artificer",
                GenericName = "FaceA" + i,
                SpecificName = "FaceC" + i
            });

            face.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Artificer",
                GenericName = "FaceB" + i,
                SpecificName = "FaceD" + i
            });

            face.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Slugpup",
                GenericName = "FaceA" + i,
                SpecificName = "PFaceA" + i
            });

            face.SlugcatSpecificReplacements.Add(new()
            {
                Slugcat = "Slugpup",
                GenericName = "FaceB" + i,
                SpecificName = "PFaceB" + i
            });
        }

        AvailableSprites.Add(face);

        var body = new AvailableSprite()
        {
            Name = "BODY",
            Description = "Body",
            GallerySprite = "BodyA",
            RequiredSprites = new() { "BodyA" },
            ExcludedSlugcatsSprites = new() { { "Watcher", "BODY" } }
        };

        //Skip body watcher, contains a shader that don't allows sprites or colors, this is for the ability
        if(!body.ExcludedSlugcatsSprites.ContainsKey("Watcher"))
        {
            AvailableSprites.Add(body);
        }

        AvailableSprites.Add(new()
        {
            Name = "ARMS",
            Description = "Arms",
            GallerySprite = "PlayerArm12",
            RequiredSprites = new() { "PlayerArm0", "PlayerArm1", "PlayerArm2", "PlayerArm3", "PlayerArm4", "PlayerArm5", "PlayerArm6", "PlayerArm7", "PlayerArm8", "PlayerArm9", "PlayerArm10", "PlayerArm11", "PlayerArm12", "OnTopOfTerrainHand", "OnTopOfTerrainHand2" }
        });

        AvailableSprites.Add(new()
        {
            Name = "HIPS",
            Description = "Hips",
            GallerySprite = "HipsA",
            RequiredSprites = new() { "HipsA" }
        });

        AvailableSprites.Add(new()
        {
            Name = "LEGS",
            Description = "Legs",
            GallerySprite = "LegsA0",
            RequiredSprites = new() { "LegsA0", "LegsA1", "LegsA2", "LegsA3", "LegsA4", "LegsA5", "LegsA6", "LegsAAir0", "LegsAAir1", "LegsAClimbing0", "LegsAClimbing1", "LegsAClimbing2", "LegsAClimbing3", "LegsAClimbing4", "LegsAClimbing5", "LegsAClimbing6", "LegsACrawling0", "LegsACrawling1", "LegsACrawling2", "LegsACrawling3", "LegsACrawling4", "LegsACrawling5", "LegsAOnPole0", "LegsAOnPole1", "LegsAOnPole2", "LegsAOnPole3", "LegsAOnPole4", "LegsAOnPole5", "LegsAOnPole6", "LegsAPole", "LegsAVerticalPole", "LegsAWall" }
        });

        AvailableSprites.Add(new()
        {
            Name = "TAIL",
            Description = "Tail",
            GallerySprite = "TailTexture",
            RequiredSprites = new() { "TailTexture" }
        });

        AvailableSprites.Add(new()
        {
            Name = "FACESCAR",
            Description = "Face Scar",
            GallerySprite = "MushroomA",
            RequiredSprites = new() { "MushroomA" },
            Slugcats = new() { "Artificer" }
        });

        AvailableSprites.Add(new()
        {
            Name = "GILLS",
            Description = "Gills",
            GallerySprite = "LizardScaleA3",
            RequiredSprites = new() { "LizardScaleA3", "LizardScaleB3" },
            Slugcats = new() { "Rivulet" }
        });

        AvailableSprites.Add(new()
        {
            Name = "TAILSPECKLES",
            Description = "Tail Speckles",
            GallerySprite = "tinyStar",
            RequiredSprites = new() { "tinyStar"},
            Slugcats = new() { "Spear" }
        });

        AvailableSprites.Add(new()
        {
            Name = "ASCENSION",
            Description = "Ascension",
            GallerySprite = "guardEye",
            RequiredSprites = new() { "guardEye", "WormEye" },
            Slugcats = new() { "Saint" }
        });

        AvailableSprites.Add(new()
        {
            Name = "PIXEL",
            Description = "The Mark",
            GallerySprite = "pixel",
            RequiredSprites = new() { "pixel" }
        });
    }

    public class AvailableSprite
    {
        public string Name;
        public string Description;
        public string GallerySprite;
        public List<string> Slugcats = new();
        public List<string> RequiredSprites = new();
        public List<SlugcatSpecificReplacement> SlugcatSpecificReplacements = new();

        public Dictionary<string, string> ExcludedSlugcatsSprites = new();

        public class SlugcatSpecificReplacement
        {
            public string Slugcat;
            public string SpecificName;
            public string GenericName;
        }
    }
}