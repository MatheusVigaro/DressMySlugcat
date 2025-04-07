namespace DressMySlugcat.NoirEars;

public partial class NoirEars
{
    public static void PlayerGraphicsOnctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (!self.player.TryGetEarsData(out var earsData)) return;

        foreach (var ear in earsData.Ears)
        {
            ear[0] = new TailSegment(self, 4.5f, 4f, null, 0.85f, 1f, 1f, true);
            ear[1] = new TailSegment(self, 1.5f, 7f, ear[0], 0.85f, 1f, 0.5f, true);
        }

        var partsToAdd = new List<BodyPart>();
        partsToAdd.AddRange(self.bodyParts);
        partsToAdd.AddRange(earsData.Ears[0]);
        partsToAdd.AddRange(earsData.Ears[1]);
        self.bodyParts = partsToAdd.ToArray();
    }

    public static void PlayerGraphicsOnInitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam)
    {
        if (!self.player.TryGetEarsData(out var earsData))
        {
            orig(self, sleaser, rcam);
            return;
        }

        if (!rcam.game.DEBUGMODE)
        {
            earsData.CallingAddToContainerFromOrigInitiateSprites = true;
        }

        orig(self, sleaser, rcam);

        if (!rcam.game.DEBUGMODE)
        {
            earsData.TotalSprites = sleaser.sprites.Length;
            earsData.EarSpr[0] = earsData.TotalSprites;
            earsData.EarSpr[1] = earsData.TotalSprites + 1;
            Array.Resize(ref sleaser.sprites, earsData.TotalSprites + EarsData.NewSprites);

            #region Init Ear arrays
            var earArray = new TriangleMesh.Triangle[(earsData.Ears[0].Length - 1) * 4 + 1];
            for (var i = 0; i < earsData.Ears[0].Length - 1; i++)
            {
                var indexTimesFour = i * 4;
                for (var j = 0; j <= 3; j++)
                {
                    earArray[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
                }
            }
            earArray[(earsData.Ears[0].Length - 1) * 4] = new TriangleMesh.Triangle((earsData.Ears[0].Length - 1) * 4, (earsData.Ears[0].Length - 1) * 4 + 1, (earsData.Ears[0].Length - 1) * 4 + 2);
            foreach (var sprNum in earsData.EarSpr)
            {
                sleaser.sprites[sprNum] = new TriangleMesh(Ears, earArray, false, false);
            }
            #endregion

            earsData.CallingAddToContainerFromOrigInitiateSprites = false;
            self.AddToContainer(sleaser, rcam, null);
        }
    }

    public static void PlayerGraphicsOnAddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, FContainer newcontatiner)
    {
        if (!self.player.TryGetEarsData(out var noirData))
        {
            orig(self, sleaser, rcam, newcontatiner);
            return;
        }

        if (noirData.CallingAddToContainerFromOrigInitiateSprites) return;

        orig(self, sleaser, rcam, newcontatiner);

        if (!rcam.game.DEBUGMODE)
        {
            var container = rcam.ReturnFContainer("Midground");
            container.AddChild(sleaser.sprites[noirData.EarSpr[0]]);
            container.AddChild(sleaser.sprites[noirData.EarSpr[1]]);

            sleaser.sprites[noirData.EarSpr[0]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[noirData.EarSpr[1]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
        }
    }

    public static void PlayerGraphicsOnDrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (!self.player.TryGetEarsData(out var earsData)) return;
        if (rcam.room.game.DEBUGMODE) return;

        MoveMeshes(earsData, sleaser, timestacker, campos);

        #region Moving Sprites to front/back
        if (earsData.FlipDirection == 1)
        {
            sleaser.sprites[earsData.EarSpr[0]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
            sleaser.sprites[earsData.EarSpr[1]].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
        }
        else
        {
            sleaser.sprites[earsData.EarSpr[0]].MoveBehindOtherNode(sleaser.sprites[BodySpr]);
            sleaser.sprites[earsData.EarSpr[1]].MoveInFrontOfOtherNode(sleaser.sprites[HeadSpr]);
        }
        #endregion
    }

    private static void PlayerGraphicsOnApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
    {
        orig(self, sleaser, rcam, palette);
        if (!self.player.TryGetEarsData(out var earsData)) return;

        for (var i = 0; i < earsData.EarSpr.Length; i++)
        {
            ApplyMeshTexture(sleaser.sprites[earsData.EarSpr[i]] as TriangleMesh);
            sleaser.sprites[earsData.EarSpr[i]].shader = FShader.defaultShader;
        }
    }

    public static void PlayerGraphicsOnReset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
    {
        orig(self);
        if (!self.player.TryGetEarsData(out var earsData)) return;

        for (var i = 0; i < 2; i++)
        {
            earsData.Ears[i][0].Reset(EarAttachPos(earsData, i, 1f));
            earsData.Ears[i][1].Reset(EarAttachPos(earsData, i, 1f));
        }
    }
    
    //Helpers
    private static void EarsUpdate(EarsData earsData)
    {
        for (var i = 0; i < 2; i++)
        {
            earsData.Ears[i][0].connectedPoint = EarAttachPos(earsData, i, 1f);
            earsData.Ears[i][0].Update();
            earsData.Ears[i][1].Update();
        }
    }

    private static void MoveMeshes(EarsData earsData, RoomCamera.SpriteLeaser sleaser, float timestacker, Vector2 campos)
    {
        //I swear, I TRIED optimizing this...
        var earAttachPos = EarAttachPos(earsData, 0, timestacker);
        var earRad = earsData.Ears[0][0].rad;
        for (var index = 0; index < earsData.Ears[0].Length; ++index)
        {
            var earMesh = (TriangleMesh)sleaser.sprites[earsData.EarSpr[0]];
            var earPos = Vector2.Lerp(earsData.Ears[0][index].lastPos, earsData.Ears[0][index].pos, timestacker);
            var normalized = (earPos - earAttachPos).normalized;
            var vector2_3 = Custom.PerpendicularVector(normalized);
            var distance = Vector2.Distance(earPos, earAttachPos) / 5f;
            if (index == 0) distance = 0.0f;
            earMesh.MoveVertice(index * 4, earAttachPos - earsData.EarsFlip[0] * vector2_3 * earRad + normalized * distance - campos);
            earMesh.MoveVertice(index * 4 + 1, earAttachPos + earsData.EarsFlip[0] * vector2_3 * earRad + normalized * distance - campos);
            if (index < earsData.Ears[0].Length - 1)
            {
                earMesh.MoveVertice(index * 4 + 2, earPos - earsData.EarsFlip[0] * vector2_3 * earsData.Ears[0][index].StretchedRad - normalized * distance - campos);
                earMesh.MoveVertice(index * 4 + 3, earPos + earsData.EarsFlip[0] * vector2_3 * earsData.Ears[0][index].StretchedRad - normalized * distance - campos);
            }
            else
                earMesh.MoveVertice(index * 4 + 2, earPos - campos);
            earRad = earsData.Ears[0][index].StretchedRad;
            earAttachPos = earPos;
        }


        var ear2AttachPos = EarAttachPos(earsData, 1, timestacker);
        var ear2Rad = earsData.Ears[1][0].rad;
        for (var index = 0; index < earsData.Ears[1].Length; ++index)
        {

            var ear2Mesh = (TriangleMesh)sleaser.sprites[earsData.EarSpr[1]];
            var ear2Pos = Vector2.Lerp(earsData.Ears[1][index].lastPos, earsData.Ears[1][index].pos, timestacker);
            var normalized = (ear2Pos - ear2AttachPos).normalized;
            var vector2_3 = Custom.PerpendicularVector(normalized);
            var distance = Vector2.Distance(ear2Pos, ear2AttachPos) / 5f;
            if (index == 0) distance = 0.0f;
            ear2Mesh.MoveVertice(index * 4, ear2AttachPos + earsData.EarsFlip[1] * vector2_3 * ear2Rad + normalized * distance - campos);
            ear2Mesh.MoveVertice(index * 4 + 1, ear2AttachPos - earsData.EarsFlip[1] * vector2_3 * ear2Rad + normalized * distance - campos);
            if (index < earsData.Ears[1].Length - 1)
            {
                ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos + earsData.EarsFlip[1] * vector2_3 * earsData.Ears[1][index].StretchedRad - normalized * distance - campos);
                ear2Mesh.MoveVertice(index * 4 + 3, ear2Pos - earsData.EarsFlip[1] * vector2_3 * earsData.Ears[1][index].StretchedRad - normalized * distance - campos);
            }
            else
                ear2Mesh.MoveVertice(index * 4 + 2, ear2Pos - campos);
            ear2Rad = earsData.Ears[1][index].StretchedRad;
            ear2AttachPos = ear2Pos;
        }
    }

    private static void ApplyMeshTexture(TriangleMesh triMesh) //Code adapted from SlimeCubed's CustomTails
    {
        if (triMesh == null) return;

        if (triMesh.verticeColors == null || triMesh.verticeColors.Length != triMesh.vertices.Length)
        {
            triMesh.verticeColors = new Color[triMesh.vertices.Length];
        }
        triMesh.color = Color.white;
        triMesh.customColor = true;

        for (var j = triMesh.verticeColors.Length - 1; j >= 0; j--)
        {
            var num = (j / 2f) / (triMesh.verticeColors.Length / 2f);
            triMesh.verticeColors[j] = triMesh.color;
            Vector2 vector;
            if (j % 2 == 0)
            {
                vector = new Vector2(num, 0f);
            }
            else if (j < triMesh.verticeColors.Length - 1)
            {
                vector = new Vector2(num, 1f);
            }
            else
            {
                vector = new Vector2(1f, 0f);
            }
            vector.x = Mathf.Lerp(triMesh.element.uvBottomLeft.x, triMesh.element.uvTopRight.x, vector.x);
            vector.y = Mathf.Lerp(triMesh.element.uvBottomLeft.y, triMesh.element.uvTopRight.y, vector.y);
            triMesh.UVvertices[j] = vector;
        }
        triMesh.Refresh();
    }
}