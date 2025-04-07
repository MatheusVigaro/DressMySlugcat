namespace DressMySlugcat.NoirEars;

public static class EarsCWT
{
    private static readonly ConditionalWeakTable<AbstractCreature, NoirEars.EarsData> EarDeets = new ConditionalWeakTable<AbstractCreature, NoirEars.EarsData>();

    public static bool TryGetEarsData(this Player player, out NoirEars.EarsData earData) => TryGetEarsData(player.abstractCreature, out earData);
    public static bool TryGetEarsData(this AbstractCreature crit, out NoirEars.EarsData earData)
    {
        if ((crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat || crit.state is PlayerState) && true) //TODO add check for DMS player
        {
            earData = EarDeets.GetValue(crit, _ => new NoirEars.EarsData(crit));
            return true;
        }

        earData = null;
        return false;
    }
}

public static partial class NoirEars
{
    public class EarsData
    {
        public readonly AbstractCreature AbstractCat;
        public Player Cat => AbstractCat.realizedCreature as Player;

        public const int NewSprites = 2;
        public readonly int[] EarSpr = new int[2];
        public int TotalSprites;

        public readonly TailSegment[][] Ears =
        [
            new TailSegment[2],
            new TailSegment[2]
        ];
        public readonly int[] EarsFlip = [1, 1];
        public Vector2 LastHeadRotation;
        public bool CallingAddToContainerFromOrigInitiateSprites;

        public int FlipDirection
        {
            get
            {
                if (Cat == null) return 1;

                if (Mathf.Abs(Cat.bodyChunks[0].pos.x - Cat.bodyChunks[1].pos.x) < 2f)
                {
                    return Cat.flipDirection;
                }
                else
                {
                    return Cat.bodyChunks[0].pos.x > Cat.bodyChunks[1].pos.x ? 1 : -1;
                }
            }
        }

        public EarsData(AbstractCreature abstractCat)
        {
            AbstractCat = abstractCat;
        }
    }
}