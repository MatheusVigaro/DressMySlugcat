using System.Linq;

namespace DressMySlugcat;

public static class SpriteDefinitions
{
    public static List<AvailableSprite> AvailableSprites = [];
    public static List<Customization> SlugcatDefaults = [];
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

    public static void AddSlugcatDefault(Customization customization)
    {
        SlugcatDefaults.Add(customization);
    }
    
    //public static Customization GetSlugcatDefault(string slugcat, int playerNumber) => SlugcatDefaults.Where(x => x.Slugcat == slugcat && x.PlayerNumber == playerNumber).LastOrDefault();
    public static Customization GetSlugcatDefault(string slugcat, int playerNumber)
    {
        if (playerNumber > 3) //WW- So Myriad players past 4 can use defaults
            playerNumber = 0;

        // BW - Sprites from code slugcats were not loading
        IEnumerable<Customization> enumerable() => SlugcatDefaults.Where(x => x.Slugcat == slugcat && x.PlayerNumber == playerNumber);

        return enumerable().LastOrDefault();
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
            RequiredSprites = ["HeadA0", "HeadA1", "HeadA2", "HeadA3", "HeadA4", "HeadA5", "HeadA6", "HeadA7", "HeadA8", "HeadA9", "HeadA10", "HeadA11", "HeadA12", "HeadA13", "HeadA14", "HeadA15", "HeadA16", "HeadA17"]
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
            RequiredSprites = ["FaceA0", "FaceA1", "FaceA2", "FaceA3", "FaceA4", "FaceA5", "FaceA6", "FaceA7", "FaceA8", "FaceB0", "FaceB1", "FaceB2", "FaceB3", "FaceB4", "FaceB5", "FaceB6", "FaceB7", "FaceB8", "FaceDead", "FaceStunned"]
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
            RequiredSprites = ["BodyA"],
            ExcludedSlugcatsSprites = new() { { "Watcher", "BODY" } }
        };

        AvailableSprites.Add(body);

        AvailableSprites.Add(new()
        {
            Name = "ARMS",
            Description = "Arms",
            GallerySprite = "PlayerArm12",
            RequiredSprites = ["PlayerArm0", "PlayerArm1", "PlayerArm2", "PlayerArm3", "PlayerArm4", "PlayerArm5", "PlayerArm6", "PlayerArm7", "PlayerArm8", "PlayerArm9", "PlayerArm10", "PlayerArm11", "PlayerArm12", "OnTopOfTerrainHand", "OnTopOfTerrainHand2"]
        });

        AvailableSprites.Add(new()
        {
            Name = "HIPS",
            Description = "Hips",
            GallerySprite = "HipsA",
            RequiredSprites = ["HipsA"]
        });

        AvailableSprites.Add(new()
        {
            Name = "LEGS",
            Description = "Legs",
            GallerySprite = "LegsA0",
            RequiredSprites = ["LegsA0", "LegsA1", "LegsA2", "LegsA3", "LegsA4", "LegsA5", "LegsA6", "LegsAAir0", "LegsAAir1", "LegsAClimbing0", "LegsAClimbing1", "LegsAClimbing2", "LegsAClimbing3", "LegsAClimbing4", "LegsAClimbing5", "LegsAClimbing6", "LegsACrawling0", "LegsACrawling1", "LegsACrawling2", "LegsACrawling3", "LegsACrawling4", "LegsACrawling5", "LegsAOnPole0", "LegsAOnPole1", "LegsAOnPole2", "LegsAOnPole3", "LegsAOnPole4", "LegsAOnPole5", "LegsAOnPole6", "LegsAPole", "LegsAVerticalPole", "LegsAWall"]
        });

        AvailableSprites.Add(new()
        {
            Name = "TAIL",
            Description = "Tail",
            GallerySprite = "TailTexture",
            RequiredSprites = ["TailTexture"]
        });

        AvailableSprites.Add(new()
        {
            Name = "FACESCAR",
            Description = "Face Scar",
            GallerySprite = "MushroomA",
            RequiredSprites = ["MushroomA"],
            Slugcats = ["Artificer"]
        });

        AvailableSprites.Add(new()
        {
            Name = "GILLS",
            Description = "Gills",
            GallerySprite = "LizardScaleA3",
            RequiredSprites = ["LizardScaleA3", "LizardScaleB3"],
            Slugcats = ["Rivulet"]
        });

        AvailableSprites.Add(new()
        {
            Name = "TAILSPECKLES",
            Description = "Tail Speckles",
            GallerySprite = "tinyStar",
            RequiredSprites = ["tinyStar"],
            Slugcats = ["Spear"]
        });

        AvailableSprites.Add(new()
        {
            Name = "ASCENSION",
            Description = "Ascension",
            GallerySprite = "guardEye",
            RequiredSprites = ["guardEye", "WormEye"],
            Slugcats = ["Saint"]
        });

        AvailableSprites.Add(new()
        {
            Name = "PIXEL",
            Description = "The Mark",
            GallerySprite = "pixel",
            RequiredSprites = ["pixel"]
        });
    }

    public class AvailableSprite
    {
        public string Name;
        public string Description;
        public string GallerySprite;
        public List<string> Slugcats = [];
        public List<string> RequiredSprites = [];
        public List<SlugcatSpecificReplacement> SlugcatSpecificReplacements = [];

        public Dictionary<string, string> ExcludedSlugcatsSprites = [];

        public class SlugcatSpecificReplacement
        {
            public string Slugcat;
            public string SpecificName;
            public string GenericName;
        }
    }
}