namespace DressMySlugcat.NoirEars;

public partial class NoirEars
{
    public static void PlayerGraphicsOnUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.TryGetEarsData(out var earsData)) return;

        EarsUpdate(earsData);
        MoveEars(earsData);

        earsData.LastHeadRotation = self.head.connection.Rotation;
    }

    private static void MoveEars(EarsData earsData)
    {
        var self = (PlayerGraphics)earsData.Cat.graphicsModule;
        var earL = earsData.Ears[0];
        var earR = earsData.Ears[1];

        for (var i = 0; i < 2; i++)
        {
            earsData.Ears[i][0].vel.x *= 0.5f;
            earsData.Ears[i][0].vel.y += self.player.EffectiveRoomGravity * 0.5f;
            earsData.Ears[i][1].vel.x *= 0.3f;
            earsData.Ears[i][1].vel.y += self.player.EffectiveRoomGravity * 0.3f;
        }

        if ((self.player.animation == Player.AnimationIndex.None && self.player.input[0].x != 0) ||
            (self.player.animation == Player.AnimationIndex.StandOnBeam && self.player.input[0].x != 0) ||
            self.player.bodyMode == Player.BodyModeIndex.Crawl)
        {
            if (earsData.FlipDirection == 1)
            {
                earsData.EarsFlip[0] = 1;
                earsData.EarsFlip[1] = -1;
            }
            else
            {
                earsData.EarsFlip[0] = -1;
                earsData.EarsFlip[1] = 1;
            }

            if ((self.player.bodyMode == Player.BodyModeIndex.Crawl) && self.player.input[0].x == 0)
            {
                var earsFlpDirNeg = earsData.FlipDirection * -1;
                if (earsData.FlipDirection == 1)
                {
                    earL[0].vel.x += 0.45f * earsFlpDirNeg;
                    earL[1].vel.x += 0.45f * earsFlpDirNeg;
                    earR[0].vel.x += 0.35f * earsFlpDirNeg;
                    earR[1].vel.x += 0.35f * earsFlpDirNeg;

                    if (self.player.superLaunchJump >= 20)
                    {
                        earL[0].vel.x += 0.5f * earsFlpDirNeg;
                        earL[1].vel.x += 0.5f * earsFlpDirNeg;
                        earR[0].vel.x += 0.5f * earsFlpDirNeg;
                        earR[1].vel.x += 0.5f * earsFlpDirNeg;
                    }
                }
                else
                {
                    earL[0].vel.x += 0.35f * earsFlpDirNeg;
                    earL[1].vel.x += 0.35f * earsFlpDirNeg;
                    earR[0].vel.x += 0.45f * earsFlpDirNeg;
                    earR[1].vel.x += 0.45f * earsFlpDirNeg;

                    if (self.player.superLaunchJump >= 20)
                    {
                        earL[0].vel.x += 0.5f * earsFlpDirNeg;
                        earL[1].vel.x += 0.5f * earsFlpDirNeg;
                        earR[0].vel.x += 0.5f * earsFlpDirNeg;
                        earR[1].vel.x += 0.5f * earsFlpDirNeg;
                    }
                }
            }
        }
        else
        {
            earsData.EarsFlip[0] = 1;
            earsData.EarsFlip[1] = 1;

            //Push ears to the side when idle
            earL[1].vel.x -= 0.5f;
            earR[1].vel.x += 0.5f;
        }
    }
}