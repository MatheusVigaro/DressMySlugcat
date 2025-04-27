namespace DressMySlugcat;

public class GalleryDialog : Dialog, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
{
    public SimpleButton cancelButton;
    public RoundedRect border;
    public RoundedRect[,] spriteBoxes;
    public List<SpriteSheet> spriteSheets;
    public MenuLabel[] galleryLabels;
    public FSprite[] gallerySprites;
    public SelectOneButton[] galleryButtons;
    public SymbolButton leftPage;
    public SymbolButton rightPage;
    public MenuLabel pageLabel;
    public CheckBox useDefaults;
    public CheckBox useEntireSet;
    public int currentSelection = -1;
    public int currentPageNumber;
    public FancyMenu owner;
    public string spriteName;
    public int pageCount;

    public int columns = 4;
    public int rows = 3;

    public int paddingX = 18;
    public int paddingY = 70;
    public int boxMargin = 15;
    public int boxSize = 180;
    public int labelHeight = 20;

    public SelectOneButton[] playerButtons;

    public GalleryDialog(string spriteName, FancyMenu owner)
        : base(owner.manager)
    {
        this.owner = owner;
        this.spriteName = spriteName;

        border = new RoundedRect(this, pages[0], new Vector2(8, 42f), new Vector2(800, 725), true);

        darkSprite.anchorX = 0f;
        darkSprite.anchorY = 0f;
        darkSprite.scaleX = border.size.x - 12f;
        darkSprite.scaleY = border.size.y - 12f;
        darkSprite.x = border.pos.x + 6f;
        darkSprite.y = border.pos.y + 6f;
        darkSprite.alpha = 1f;

        cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5 + owner.leftAnchor, darkSprite.y + 5), new Vector2(110f, 30f));
        pages[0].subObjects.Add(cancelButton);

        spriteBoxes = new RoundedRect[4, 3];

        var label = new MenuLabel(this, pages[0], spriteName, new Vector2(((paddingX + darkSprite.x + darkSprite.scaleX - 200f) * 0.5f) + owner.leftAnchor, darkSprite.y + darkSprite.scaleY - 30f), new Vector2(200f, 30f), true);
        pages[0].subObjects.Add(label);

        spriteSheets = [];
        foreach (var spriteSheet in SpriteSheets)
        {
            if (!spriteSheet.AvailableSprites.Any(x => x.Name == spriteName))
            {
                continue;
            }

            spriteSheets.Add(spriteSheet);
        }

        pageCount = (spriteSheets.Count / (rows * columns)) + 1;

        pageLabel = new MenuLabel(this, pages[0], "", cancelButton.pos + new Vector2(336, 0), new Vector2(102, 30), true);
        pages[0].subObjects.Add(pageLabel);

        leftPage = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "LEFT_PAGE", cancelButton.pos + new Vector2(300, -6));
        leftPage.symbolSprite.rotation = 270f;
        leftPage.size = new Vector2(36f, 36f);
        leftPage.roundedRect.size = leftPage.size;
        pages[0].subObjects.Add(leftPage);

        rightPage = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "RIGHT_PAGE", cancelButton.pos + new Vector2(438, -6));
        rightPage.symbolSprite.rotation = 90f;
        rightPage.size = new Vector2(36f, 36f);
        rightPage.roundedRect.size = rightPage.size;
        pages[0].subObjects.Add(rightPage);

        var sheet = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex).CustomSprite(spriteName)?.SpriteSheet;
        if (sheet != null)
        {
            currentPageNumber = spriteSheets.IndexOf(sheet) / (rows * columns);
        }

        useDefaults = new CheckBox(this, pages[0], this, rightPage.pos + new Vector2(rightPage.size.x + 5, 6), 40, "Use Defaults", "USE_DEFAULTS", true);
        pages[0].subObjects.Add(useDefaults);

        useEntireSet = new CheckBox(this, pages[0], this, rightPage.pos + new Vector2(rightPage.size.x + 125, 6), 40, "Use Entire Set", "USE_ENTIRE_SET", true);
        pages[0].subObjects.Add(useEntireSet);

        SetupGallery();
        LoadPage(currentPageNumber);

        if (currentSelection < 0)
        {
            currentSelection = 0;
        }

        var resetButton = new SimpleButton(this, pages[0], "RELOAD ATLASES", "RELOAD_ATLASES_GALLERY", owner.resetButton.pos, owner.resetButton.size);
        pages[0].subObjects.Add(resetButton);

        for (var i = 0; i < owner.playerButtons.Length; i++)
        {
            var ownerButton = owner.playerButtons[i];
            var button = new SelectOneButton(this, pages[0], ownerButton.menuLabel.text, ownerButton.signalText, ownerButton.pos, ownerButton.size, playerButtons, i);
            pages[0].subObjects.Add(button);
        }
    }

    public override void Update()
    {
        base.Update();
        if (owner.InGame)
        {
            if (RWInput.CheckPauseButton(0))
            {
                owner.Singal(owner.backObject, "BACK");
                manager.StopSideProcess(this);
            }
        }
    }

    public void SetupGallery()
    {
        galleryButtons = new SelectOneButton[rows * columns];
        gallerySprites = new FSprite[rows * columns];
        galleryLabels = new MenuLabel[rows * columns];

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                var n = (y * columns) + x;
                var pos = new Vector2(border.pos.x + paddingX + (boxMargin * x) + (boxSize * x) + owner.leftAnchor, 768 - (paddingY + (boxMargin * y) + (boxSize * y) + (labelHeight * y) + boxSize));
                var size = new Vector2(boxSize, boxSize);

                galleryLabels[n] = new MenuLabel(this, pages[0], "", pos + new Vector2(0, size.y + 5), new Vector2(size.x, 20f), true);
                pages[0].subObjects.Add(galleryLabels[n]);

                gallerySprites[n] = new FSprite("pixel");

                var sprite = gallerySprites[n];
                container.AddChild(sprite);

                sprite.x = pos.x + 2 + ((size.x - 4) / 2);
                sprite.y = pos.y + 2 + ((size.x - 4) / 2);
                sprite.anchorX = 0.5f;
                sprite.anchorY = 0.5f;
                if (spriteName == "HIPS")
                {
                    sprite.rotation = 180;
                }
                galleryButtons[n] = new SelectOneButton(this, pages[0], "", "", pos - new Vector2(2, 2), size + new Vector2(2, 2), galleryButtons, n);

                pages[0].subObjects.Add(galleryButtons[n]);

                gallerySprites[n].isVisible = false;
                galleryButtons[n].inactive = true;
                galleryLabels[n].text = string.Empty;
            }
        }
    }

    public void LoadPage(int page)
    {
        currentSelection = -1;
        currentPageNumber = page;

        pageLabel.text = page + 1 + "/" + pageCount;
        leftPage.inactive = page == 0;
        rightPage.inactive = page >= pageCount - 1;

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < columns; x++)
            {
                var spritePosition = (y * columns) + x;
                var spriteNumber = (page * columns * rows) + spritePosition;
                if (spriteNumber < spriteSheets.Count)
                {
                    var spriteSheet = spriteSheets[spriteNumber];
                    var pos = new Vector2(border.pos.x + paddingX + (boxMargin * x) + (boxSize * x), 768 - (paddingY + (boxMargin * y) + (boxSize * y) + (labelHeight * y) + boxSize));
                    var size = new Vector2(boxSize, boxSize);

                    var sprite = gallerySprites[spritePosition];

                    var slugcatDefault = SpriteDefinitions.GetSlugcatDefault(owner.selectedSlugcat, owner.selectedPlayerIndex)?.CustomSprite(spriteName);

                    if (spriteSheet == SpriteSheet.GetDefault() && slugcatDefault != null)
                    {
                        var defaulSpritesheet = SpriteSheet.Get(slugcatDefault.SpriteSheetID);
                        try
                        {
                            sprite.element = defaulSpritesheet.TrimmedElements[SpriteDefinitions.AvailableSprites.Where(x => x.Name == spriteName).FirstOrDefault().GallerySprite];
                        }
                        catch
                        {
                            Debug.Log("FAILED TO FIND SPRITE! defaulting to backup");
                            sprite.element = spriteSheet.TrimmedElements[SpriteDefinitions.AvailableSprites.Where(x => x.Name == spriteName).FirstOrDefault().GallerySprite];
                        }
                    }
                    else
                    {
                        sprite.element = spriteSheet.TrimmedElements[SpriteDefinitions.AvailableSprites.Where(x => x.Name == spriteName).FirstOrDefault().GallerySprite];
                    }

                    sprite.x = pos.x + 2 + ((size.x - 4) / 2);
                    sprite.y = pos.y + 2 + ((size.x - 4) / 2);
                    sprite.anchorX = 0.5f;
                    sprite.anchorY = 0.5f;

                    var element = sprite.element;
                    if (element.sourceSize.x > element.sourceSize.y)
                    {
                        var targetSize = size.x - 4f;
                        var scale = targetSize / element.sourceSize.x;

                        sprite.scaleX = scale;
                        sprite.scaleY = scale;
                        //var ySize = element.sourceSize.x * scale;
                        //sprite.y = sprite.y + (targetSize - ySize) / 2;
                    }
                    else
                    {
                        var targetSize = size.y - 4f;
                        var scale = targetSize / element.sourceSize.y;

                        sprite.scaleX = scale;
                        sprite.scaleY = scale;
                        //var xSize = element.sourceSize.y * scale;
                        //sprite.x = sprite.x + (targetSize - xSize) / 2;
                    }

                    gallerySprites[spritePosition].isVisible = true;

                    galleryButtons[spritePosition].signalText = "SELECTED_" + spriteSheet.ID;
                    galleryButtons[spritePosition].inactive = false;

                    galleryLabels[spritePosition].text = spriteSheet.Name;

                    if (Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false).CustomSprite(spriteName)?.SpriteSheetID == spriteSheet.ID)
                    {
                        currentSelection = spritePosition;
                    }
                }
                else
                {
                    gallerySprites[spritePosition].isVisible = false;
                    galleryButtons[spritePosition].inactive = true;
                    galleryLabels[spritePosition].text = "";
                }
            }
        }
    }

    public int GetCurrentlySelectedOfSeries(string series)
    {
        if (series.StartsWith("SELECTED_"))
        {
            return currentSelection;
        }
        else if (series.StartsWith("PLAYER_"))
        {
            return owner.selectedPlayerIndex;
        }
        return -1;
    }

    public void SetCurrentlySelectedOfSeries(string series, int to)
    {
        if (series.StartsWith("SELECTED_") && currentSelection != to)
        {
            var spriteNumber = (currentPageNumber * columns * rows) + to;
            if (spriteNumber < spriteSheets.Count)
            {
                currentSelection = to;
                var spriteSheet = spriteSheets[(currentPageNumber * columns * rows) + to];
                var customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false);

                if (owner.useEntireSet)
                {
                    var slugcatSprites = spriteSheet.AvailableSprites.Where(x => x.Slugcats.Count == 0 || x.Slugcats.Contains(owner.selectedSlugcat)).ToList();
                    foreach (var sprite in slugcatSprites)
                    {
                        var customSprite = customization.CustomSprite(sprite.Name, true);

                        if (owner.useDefaults && spriteSheet.DefaultColors.TryGetValue(sprite.Name, out var color) && color != default && color.a != 0)
                        {
                            customSprite.Color = color;
                        }

                        customSprite.SpriteSheetID = spriteSheet.ID;
                    }
                }
                else
                {
                    var customSprite = customization.CustomSprite(spriteName, true);

                    if (owner.useDefaults && spriteSheet.DefaultColors.TryGetValue(spriteName, out var color) && color != default && color.a != 0)
                    {
                        customSprite.Color = color;
                    }

                    customSprite.SpriteSheetID = spriteSheet.ID;
                }

                if (owner.useDefaults)
                {
                    if (spriteSheet.DefaultTail.IsCustom)
                    {
                        customization.CustomTail.Length = spriteSheet.DefaultTail.Length;
                        customization.CustomTail.Wideness = spriteSheet.DefaultTail.Wideness;
                        customization.CustomTail.Roundness = spriteSheet.DefaultTail.Roundness;
                        customization.CustomTail.Lift = spriteSheet.DefaultTail.Lift;
                    }

                    if (spriteSheet.DefaultTail.Color != default)
                    {
                        customization.CustomTail.Color = spriteSheet.DefaultTail.Color;
                    }
                }

                owner.UpdateControls();
                owner.slugcatDummy.UpdateSprites();
            }
        }
        if (series.StartsWith("PLAYER_"))
        {
            owner.selectedPlayerIndex = to;

            LoadPage(currentPageNumber);
            owner.UpdateControls();
            owner.slugcatDummy.UpdateSprites();
        }
    }

    public override void Singal(MenuObject sender, string message)
    {
        if (message == "BACK")
        {
            PlaySound(SoundID.MENU_Switch_Page_Out);
            foreach (var sprite in gallerySprites)
            {
                sprite?.RemoveFromContainer();
            }

            manager.StopSideProcess(this);
        }
        else if (message == "RELOAD_ATLASES_GALLERY")
        {
            Singal(sender, "BACK");
            owner.Singal(sender, "RELOAD_ATLASES");
            owner.Singal(sender, "SPRITE_SELECTOR_" + spriteName);
        }
        else if (message == "LEFT_PAGE")
        {
            if ((sender as SymbolButton).inactive)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
            }
            else
            {
                PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                LoadPage(currentPageNumber - 1);
            }
        }
        else if (message == "RIGHT_PAGE")
        {
            if ((sender as SymbolButton).inactive)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
            }
            else
            {
                PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                LoadPage(currentPageNumber + 1);
            }
        }
    }

    public bool GetChecked(CheckBox box)
    {
        return box.IDString switch
        {
            "USE_DEFAULTS" => owner.useDefaults,
            "USE_ENTIRE_SET" => owner.useEntireSet,
            _ => false,
        };
    }

    public void SetChecked(CheckBox box, bool c)
    {
        switch (box.IDString)
        {
            case "USE_DEFAULTS":
                owner.useDefaults = c;
                break;
            case "USE_ENTIRE_SET":
                owner.useEntireSet = c;
                break;
        }
    }
}
