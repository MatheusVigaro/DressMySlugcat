using DressMySlugcat.Hooks;

namespace DressMySlugcat;

public class FancyMenu : Dialog, SelectOneButton.SelectOneButtonOwner
{
    public SimpleButton backButton;
    public SimpleButton resetButton;
    public SimpleButton copyButton;
    public SimpleButton pasteButton;
    public SimpleButton defaultsButton;
    public RoundedRect textBoxBorder;
    public FSprite textBoxBack;
    public PlayerGraphicsDummy slugcatDummy;
    public Dictionary<string, SimpleButton> selectSpriteButtons = [];
    public Dictionary<string, MenuLabel> selectSpriteLabels = [];
    public Dictionary<string, SimpleButton> customizeSpriteButtons = [];
    public string selectedSlugcat;
    public bool useDefaults = true;
    public bool useEntireSet = false;

    public PauseMenu owner;
    public bool InGame => owner != null;

    public SelectOneButton[] slugcatButtons;
    public int selectedSlugcatIndex;
    public SelectOneButton[] playerButtons;
    public int selectedPlayerIndex;

    public List<string> slugcatNames;

    public int pageCount;
    public int currentSlugcatPage;
    public int slugcatsPerPage = 18;
    public SymbolButton leftPage;
    public SymbolButton rightPage;
    public MenuLabel pageLabel;

    public readonly float leftAnchor;
    public readonly float rightAnchor;

    //COPY PASTE MEMORY
    private Customization copiedCustomization = Customization.For("White", 1, false).Copy();

    public FancyMenu(ProcessManager manager, PauseMenu owner = null) : base(manager)
    {
        leftAnchor = (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
        rightAnchor = 1366f - leftAnchor;

        this.owner = owner;
        SaveManager.InitSlugcatCustomizations();
        slugcatNames = Utils.ValidSlugcatNames;
        pageCount = Mathf.CeilToInt((float)slugcatNames.Count / slugcatsPerPage);

        selectedSlugcat = slugcatNames.FirstOrDefault();

        pages.Add(new Page(this, null, "main", 0));

        if (!InGame)
        {
            scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
            pages[0].subObjects.Add(scene);

            mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        }

        darkSprite.color = new Color(0f, 0f, 0f);
        darkSprite.anchorX = 0f;
        darkSprite.anchorY = 0f;
        darkSprite.scaleX = 1368f;
        darkSprite.scaleY = 770f;
        darkSprite.x = -1f;
        darkSprite.y = -1f;
        if (InGame)
        {
            darkSprite.alpha = 0f;
        }
        else
        {
            darkSprite.alpha = 0.5f;
        }
        pages[0].Container.AddChild(darkSprite);

        backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(leftAnchor + 15f, 50f), new Vector2(220f, 30f));
        pages[0].subObjects.Add(backButton);

        backObject = backButton;
        backButton.nextSelectable[0] = backButton;
        backButton.nextSelectable[2] = backButton;
        textBoxBorder = new RoundedRect(this, pages[0], new Vector2(leftAnchor + 255f, 50f), new Vector2(1050f - (1366f - manager.rainWorld.options.ScreenSize.x), 700f), true);

        textBoxBack = new FSprite("pixel")
        {
            color = new Color(0f, 0f, 0f),
            anchorX = 0f,
            anchorY = 0f,
            scaleX = textBoxBorder.size.x - 12f,
            scaleY = textBoxBorder.size.y - 12f,
            x = textBoxBorder.pos.x + 6f - ((1366f - manager.rainWorld.options.ScreenSize.x) / 2f),
            y = textBoxBorder.pos.y + 6f,
            alpha = 0.65f
        };
        infoLabel.x = Mathf.Ceil(textBoxBack.x + (textBoxBack.scaleX / 2f));
        pages[0].Container.AddChild(textBoxBack);
        pages[0].subObjects.Add(textBoxBorder);

        slugcatDummy = new PlayerGraphicsDummy(this)
        {
            SlugcatPosition = new Vector2(133f - (leftAnchor / 4f), 70f)
        };
        slugcatDummy.Container.scale = 8f;

        //playerButtons = new SelectOneButton[4];
        playerButtons = new SelectOneButton[this.manager.rainWorld.options.controls.Length]; //-WW -DYNAMIC LENGTH
        for (int i = 0; i < playerButtons.Length; i++)
        {
            float startPos = playerButtons.Length > 14 ? -150 : 0; //Don't cut off 15 and 16 if they exist
            var playerButton = new SelectOneButton(this, pages[0], "Player " + (i + 1), "PLAYER_" + i, textBoxBorder.pos + new Vector2(startPos + (65 * i), -40), new Vector2(60, 30), playerButtons, i);
            pages[0].subObjects.Add(playerButton);
            playerButtons[i] = playerButton;
        }

        SetupSlugcatPages();
        LoadSlugcatPage(0);

        UpdateControls();

        resetButton = new SimpleButton(this, pages[0], "RELOAD ATLASES", "RELOAD_ATLASES", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) - new Vector2(160, 40), new Vector2(160f, 30f));
        pages[0].subObjects.Add(resetButton);

        defaultsButton = new SimpleButton(this, pages[0], "RESET", "CUST_DEFAULTS", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) + new Vector2(-210, 20), new Vector2(60f, 30f));
        pages[0].subObjects.Add(defaultsButton);

        copyButton = new SimpleButton(this, pages[0], "COPY", "CUST_COPY", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) + new Vector2(-140, 20), new Vector2(60f, 30f));
        pages[0].subObjects.Add(copyButton);

        pasteButton = new SimpleButton(this, pages[0], "PASTE", "CUST_PASTE", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) + new Vector2(-70, 20), new Vector2(60f, 30f));
        pages[0].subObjects.Add(pasteButton);
        pasteButton.inactive = true; //START OUT INACTIVE
    }

    public override void Update()
    {
        base.Update();
        if (InGame)
        {
            if (RWInput.CheckPauseButton(0))
            {
                Singal(backObject, "BACK");
            }
        }
    }

    public void SetupSlugcatPages()
    {
        slugcatButtons = new SelectOneButton[slugcatsPerPage];

        for (var i = 0; i < slugcatsPerPage; i++)
        {
            var slugcatButton = new SelectOneButton(this, pages[0], "", "", new Vector2(15f, 768f - (50f + (35f * i))), new Vector2(220f, 30f), slugcatButtons, i);
            pages[0].subObjects.Add(slugcatButton);
            slugcatButtons[i] = slugcatButton;
        }

        var offset = new Vector2(-277, 40);
        pageLabel = new MenuLabel(this, pages[0], "", backButton.pos + new Vector2(336, 0) + offset, new Vector2(102, 30), true);
        pages[0].subObjects.Add(pageLabel);

        leftPage = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "LEFT_PAGE_SLUGCAT", backButton.pos + new Vector2(300, -6) + offset);
        leftPage.symbolSprite.rotation = 270f;
        leftPage.size = new Vector2(36f, 36f);
        leftPage.roundedRect.size = leftPage.size;
        pages[0].subObjects.Add(leftPage);

        rightPage = new SymbolButton(this, pages[0], "Big_Menu_Arrow", "RIGHT_PAGE_SLUGCAT", backButton.pos + new Vector2(438, -6) + offset);
        rightPage.symbolSprite.rotation = 90f;
        rightPage.size = new Vector2(36f, 36f);
        rightPage.roundedRect.size = rightPage.size;
        pages[0].subObjects.Add(rightPage);
    }

    public void LoadSlugcatPage(int page)
    {
        selectedSlugcatIndex = -1;
        currentSlugcatPage = page;

        pageLabel.text = page + 1 + "/" + pageCount;
        leftPage.inactive = page == 0;
        rightPage.inactive = page >= pageCount - 1;

        var startingIndex = page * slugcatsPerPage;

        for (var i = 0; i < slugcatsPerPage; i++)
        {
            var currentIndex = startingIndex + i;

            if (currentIndex < slugcatNames.Count)
            {
                var slugcatName = slugcatNames[currentIndex];
                var slugcatDisplayName = slugcatName;
                ExtEnumBase.TryParse(typeof(SlugcatStats.Name), slugcatName, true, out var slugcatNameExtEnum);
                if (slugcatNameExtEnum != null)
                {
                    slugcatDisplayName = SlugcatStats.getSlugcatName((SlugcatStats.Name)slugcatNameExtEnum);
                }

                if (selectedSlugcat == slugcatName)
                {
                    selectedSlugcatIndex = i;
                }

                slugcatButtons[i].inactive = false;
                slugcatButtons[i].menuLabel.text = slugcatDisplayName;
                slugcatButtons[i].signalText = "SLUGCAT_" + slugcatName;
                slugcatButtons[i].pos.x = leftAnchor + 15f;
                slugcatButtons[i].lastPos.x = leftAnchor + 15f;
            }
            else
            {
                slugcatButtons[i].inactive = true;
                slugcatButtons[i].menuLabel.text = "";
                slugcatButtons[i].signalText = "";
                slugcatButtons[i].pos.x = -1000;
                slugcatButtons[i].lastPos.x = -1000;
            }
        }
    }

    public void UpdateControls()
    {
        foreach (var label in selectSpriteLabels.Values)
        {
            label.RemoveSprites();
            pages[0].RemoveSubObject(label);
        }
        selectSpriteLabels.Clear();

        foreach (var button in selectSpriteButtons.Values)
        {
            button.RemoveSprites();
            pages[0].RemoveSubObject(button);
        }
        selectSpriteButtons.Clear();

        foreach (var button in customizeSpriteButtons.Values)
        {
            button.RemoveSprites();
            pages[0].RemoveSubObject(button);
        }
        customizeSpriteButtons.Clear();

        var internalTopLeft = new Vector2(leftAnchor + 190f, 710f);
        var availableSprites = SpriteDefinitions.AvailableSprites.Where(x => !x.ExcludedSlugcatsSprites.ContainsKey(selectedSlugcat) && (x.Slugcats.Count == 0 || x.Slugcats.Contains(selectedSlugcat))).ToList();

        for (var i = 0; i < availableSprites.Count; i++)
        {
            var sprite = availableSprites[i];

            //TO HELP FIX HUGE SPRITELISTS THAT GO OFFSCREEN
            float iHeight = i;
            float xPad = 0f;
            if (i >= 10)
            {
                iHeight -= 10; //I'm sorry if you've got more than 20 that's on you
                xPad = 300f;
            }

            var label = new MenuLabel(this, pages[0], sprite.Description, internalTopLeft + new Vector2(72 + xPad, iHeight * -69f), new Vector2(200f, 30f), bigText: true);
            selectSpriteLabels[sprite.Name] = label;
            pages[0].subObjects.Add(label);

            var button = new SimpleButton(this, pages[0], "", "SPRITE_SELECTOR_" + sprite.Name, internalTopLeft + new Vector2(80 + xPad, (iHeight * -69) - 30), new Vector2(180f, 30f));
            selectSpriteButtons[sprite.Name] = button;
            pages[0].subObjects.Add(button);

            button = new SimpleButton(this, pages[0], "Customize", sprite.Name == "TAIL" ? "TAIL_CUSTOMIZER" : "SPRITE_CUSTOMIZER_" + sprite.Name, internalTopLeft + new Vector2(80 + 190 + xPad, (iHeight * -69) - 30), new Vector2(70f, 30f));
            customizeSpriteButtons.Add(sprite.Name, button);
            pages[0].subObjects.Add(button);
        }

        UpdateSpriteButtonsText();
    }

    public void UpdateSpriteButtonsText()
    {
        var customization = Customization.For(selectedSlugcat, selectedPlayerIndex, false);
        foreach (var key in selectSpriteButtons.Keys)
        {
            var customSprite = customization?.CustomSprite(key);
            SpriteSheet sheet = customSprite?.SpriteSheet;

            sheet ??= SpriteSheet.GetDefault();

            selectSpriteButtons[key].menuLabel.text = sheet.Name;
        }
    }

    public int GetCurrentlySelectedOfSeries(string series)
    {
        if (series.StartsWith("SLUGCAT_"))
        {
            return selectedSlugcatIndex;
        }
        else if (series.StartsWith("PLAYER_"))
        {
            return selectedPlayerIndex;
        }

        return -1;
    }

    public void SetCurrentlySelectedOfSeries(string series, int to)
    {
        if (series.StartsWith("SLUGCAT_") && selectedSlugcatIndex != to)
        {
            selectedSlugcatIndex = to;
            selectedSlugcat = slugcatNames[to + (currentSlugcatPage * slugcatsPerPage)];

            UpdateControls();
            slugcatDummy.UpdateSprites();
        }
        else if (series.StartsWith("PLAYER_") && selectedPlayerIndex != to)
        {
            selectedPlayerIndex = to;

            UpdateControls();
            slugcatDummy.UpdateSprites();
        }
    }

    public override void ShutDownProcess()
    {
        base.ShutDownProcess();
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].RemoveSprites();
        }
        container.RemoveFromContainer();
        cursorContainer.RemoveFromContainer();
        slugcatDummy.Container.RemoveFromContainer();
        infoLabel?.RemoveFromContainer();
    }

    public override void Singal(MenuObject sender, string message)
    {
        if (message == "BACK")
        {
            Customization.CleanDefaults();
            SaveManager.Save();

            PlaySound(SoundID.MENU_Switch_Page_Out);
            if (InGame)
            {
                foreach (var abstractPlayer in owner.game.Players)
                {
                    if (abstractPlayer.realizedCreature is Player player && player.graphicsModule is PlayerGraphics pg)
                    {
                        if (PlayerGraphicsHooks.PlayerGraphicsData.TryGetValue(pg, out var playerGraphicsData))
                        {
                            playerGraphicsData.ScheduleForRecreation = true;
                        }
                    }
                }

                owner.container.alpha = 1;
                manager.StopSideProcess(this);
            }
            else
            {
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            }
        }

        else if (message.StartsWith("SPRITE_SELECTOR_"))
        {
            var spritename = message.Substring(16);

            PlaySound(SoundID.MENU_Player_Join_Game);
            manager.ShowDialog(new GalleryDialog(spritename, this));
        }
        else if (message == "RELOAD_ATLASES")
        {
            AtlasHooks.ReloadAtlases();
            slugcatDummy.UpdateSprites();
            PlaySound(SoundID.MENU_Player_Join_Game);
        }
        else if (message == "LEFT_PAGE_SLUGCAT")
        {
            if ((sender as SymbolButton).inactive)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
            }
            else
            {
                PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                LoadSlugcatPage(currentSlugcatPage - 1);
            }
        }
        else if (message == "RIGHT_PAGE_SLUGCAT")
        {
            if ((sender as SymbolButton).inactive)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
            }
            else
            {
                PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                LoadSlugcatPage(currentSlugcatPage + 1);
            }
        }
        else if (message == "TAIL_CUSTOMIZER")
        {
            PlaySound(SoundID.MENU_Player_Join_Game);
            manager.ShowDialog(new TailCustomizer(this));
        }
        else if (message.StartsWith("SPRITE_CUSTOMIZER_"))
        {
            var spritename = message.Substring(18);
            PlaySound(SoundID.MENU_Player_Join_Game);
            manager.ShowDialog(new SpriteCustomizer(this, spritename));
        }
        else if (message == "CUST_DEFAULTS")
        {
            PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
            Customization.ResetToDefaults(selectedSlugcat, selectedPlayerIndex);
            UpdateControls();
            slugcatDummy.UpdateSprites();
        }
        else if (message == "CUST_COPY")
        {
            copiedCustomization = Customization.For(selectedSlugcat, selectedPlayerIndex, false).Copy();
            //Debug.Log("COPIED SPRITES!!" + selectedSlugcat + " - " + selectedPlayerIndex);
            PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
            pasteButton.inactive = false;
        }
        else if (message == "CUST_PASTE")
        {
            if (pasteButton.inactive)
            {
                PlaySound(SoundID.MENU_Greyed_Out_Button_Clicked);
            }
            else
            {
                for (var i = 0; i < SaveManager.Customizations.Count; i++)
                {
                    if (SaveManager.Customizations[i].Slugcat == selectedSlugcat && SaveManager.Customizations[i].PlayerNumber == selectedPlayerIndex)
                    {
                        var origCust = SaveManager.Customizations[i].Copy();
                        SaveManager.Customizations[i] = copiedCustomization.Copy();
                        //DON'T CHANGE THESE THOUGH, THAT'D BE BAD...
                        SaveManager.Customizations[i].PlayerNumber = origCust.PlayerNumber;
                        SaveManager.Customizations[i].Slugcat = origCust.Slugcat;
                        //DON'T COPY THE TAIL COLOR IF IT WAS DEFAULT 
                        if (copiedCustomization.CustomTail.Color == Utils.DefaultColorForSprite(copiedCustomization.Slugcat, "TAIL"))
                            SaveManager.Customizations[i].CustomTail.Color = origCust.CustomTail.Color;
                        //Debug.Log("PASTED SPRITES!!" + SaveManager.Customizations[i].PlayerNumber + " - " + SaveManager.Customizations[i].Slugcat);
                        break;
                    }
                }
                PlaySound(SoundID.MENU_Switch_Page_Out);
                UpdateControls();
                slugcatDummy.UpdateSprites();
            }
        }
    }
}