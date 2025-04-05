using RainMeadow;

namespace DressMySlugcat
{
    public class MeadowCompatibility
    {
        public static bool meadowEnabled;
        public MeadowCompatibility()
        {
            if (ModManager.ActiveMods.Any(x => x.id == "henpemaz_rainmeadow"))
            {
                meadowEnabled = true;
            }
        }

        public static bool IsMeadowSession()
        {
            if (meadowEnabled)
                return CheckMS();
            return false;
        }

        public static bool CheckMS()
        {
            if (IsMeadowSession())
                return (OnlineManager.lobby != null);
            return false;
        }

        public static bool CheckForMeadowNonselfClient(Player self)
        {
            if (IsMeadowSession())
                return CheckIsNotSelf(self);
            return false;
        }

        public static bool CheckIsNotSelf(Player self)
        {
            if (IsMeadowSession())
                return (!self.IsLocal());
            return false;
        }
    }
}
