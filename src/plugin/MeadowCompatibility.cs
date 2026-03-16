using RainMeadow;

namespace DressMySlugcat;

public class MeadowCompatibility
{
    public static bool IsMeadowSession()
    {
        if (Plugin.meadowEnabled)
            return CheckMS();
        return false;
    }

    public static bool CheckMS()
    {
        return OnlineManager.lobby != null;
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
            return !self.IsLocal();
        return false;
    }
}
