using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using DressMySlugcat.Hooks;
using System.IO;
using Menu;

namespace DressMySlugcat
{
    public class FancyMenu : Menu.Menu, SelectOneButton.SelectOneButtonOwner
    {
        public FSprite darkSprite;
        public SimpleButton backButton;
        public SimpleButton resetButton;
        public RoundedRect textBoxBorder;
        public FSprite textBoxBack;
        public PlayerGraphicsDummy slugcatDummy;
        public Dictionary<string, SimpleButton> selectSpriteButtons = new();
        public Dictionary<string, MenuLabel> selectSpriteLabels = new();
        public string selectedSlugcat;

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

        public FancyMenu(ProcessManager manager) : base (manager, Plugin.FancyMenu)
        {
            slugcatNames = SlugcatStats.Name.values.entries.Where(x => !x.StartsWith("JollyPlayer")).ToList();
            pageCount = Mathf.CeilToInt((float)slugcatNames.Count / slugcatsPerPage);

            selectedSlugcat = slugcatNames.FirstOrDefault();

            pages.Add(new Page(this, null, "main", 0));
            scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
            pages[0].subObjects.Add(scene);
            mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;

            darkSprite = new FSprite("pixel");
            darkSprite.color = new Color(0f, 0f, 0f);
            darkSprite.anchorX = 0f;
            darkSprite.anchorY = 0f;
            darkSprite.scaleX = 1368f;
            darkSprite.scaleY = 770f;
            darkSprite.x = -1f;
            darkSprite.y = -1f;
            darkSprite.alpha = 0.85f;
            pages[0].Container.AddChild(darkSprite);

            backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(15f, 50f), new Vector2(220f, 30f));
            pages[0].subObjects.Add(backButton);

            backObject = backButton;
            backButton.nextSelectable[0] = backButton;
            backButton.nextSelectable[2] = backButton;
            textBoxBorder = new RoundedRect(this, pages[0], new Vector2(255f, 50f), new Vector2(1050f, 700f), true);

            textBoxBack = new FSprite("pixel");
            textBoxBack.color = new Color(0f, 0f, 0f);
            textBoxBack.anchorX = 0f;
            textBoxBack.anchorY = 0f;
            textBoxBack.scaleX = textBoxBorder.size.x - 12f;
            textBoxBack.scaleY = textBoxBorder.size.y - 12f;
            textBoxBack.x = textBoxBorder.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
            textBoxBack.y = textBoxBorder.pos.y + 6f;
            textBoxBack.alpha = 0.65f;
            infoLabel.x = Mathf.Ceil(textBoxBack.x + textBoxBack.scaleX / 2f);
            pages[0].Container.AddChild(textBoxBack);
            pages[0].subObjects.Add(textBoxBorder);

            slugcatDummy = new PlayerGraphicsDummy(this);
            slugcatDummy.SlugcatPosition = new Vector2(133f, 70f);
            slugcatDummy.Container.scale = 8f;

            playerButtons = new SelectOneButton[4];
            for (int i = 0; i < 4; i++)
            {
                var playerButton = new SelectOneButton(this, pages[0], "Player " + (i+1), "PLAYER_" + i, textBoxBorder.pos + new Vector2(65 * i, -40), new Vector2(60, 30), playerButtons, i);
                pages[0].subObjects.Add(playerButton);
                playerButtons[i] = playerButton;
            }

            SetupSlugcatPages();
            LoadSlugcatPage(0);

            UpdateControls();

            resetButton = new SimpleButton(this, pages[0], "RELOAD ATLASES", "RELOAD_ATLASES", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) - new Vector2(160, 40), new Vector2(160f, 30f));
            pages[0].subObjects.Add(resetButton);
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

            pageLabel.text = (page + 1) + "/" + pageCount;
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
                    SlugcatStats.Name.TryParse(typeof(SlugcatStats.Name), slugcatName, true, out var slugcatNameExtEnum);
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
                    slugcatButtons[i].pos.x = 15f;
                    slugcatButtons[i].lastPos.x = 15f;
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

            var internalTopLeft = new Vector2(190f, 710f);
            var availableSprites = SpriteDefinitions.AvailableSprites.Where(x => x.Slugcats.Count == 0 || x.Slugcats.Contains(selectedSlugcat)).ToList();

            for (var i = 0; i < availableSprites.Count; i++)
            {
                var sprite = availableSprites[i];

                var label = new MenuLabel(this, pages[0], sprite.Description, internalTopLeft + new Vector2(72, i * -70f), new Vector2(200f, 30f), bigText: true);
                selectSpriteLabels[sprite.Name] = label;
                pages[0].subObjects.Add(label);

                var button = new SimpleButton(this, pages[0], "", "SPRITE_SELECTOR_" + sprite.Name, internalTopLeft + new Vector2(80, (i * -70) - 30), new Vector2(180f, 30f));
                selectSpriteButtons[sprite.Name] = button;
                pages[0].subObjects.Add(button);

                if (sprite.Name == "TAIL")
                {
                    //button = new SimpleButton(this, pages[0], "Tail Customizer", "TAIL_CUSTOMIZER", internalTopLeft + new Vector2(80 + 190, (i * -70) - 30), new Vector2(100f, 30f));
                    //selectSpriteButtons["__INTERNAL_BUTTON_TAIL_CUSTOMIZER"] = button;
                    //pages[0].subObjects.Add(button);
                }
            }

            UpdateSpriteButtonsText();
        }

        public void UpdateSpriteButtonsText()
        {
            var customization = Customization.For(selectedSlugcat, selectedPlayerIndex);
            foreach (var key in selectSpriteButtons.Keys) {
                if (key.StartsWith("__INTERNAL_BUTTON")) continue;

                var customSprite = customization.CustomSprite(key);
                SpriteSheet sheet = customSprite?.SpriteSheet;

                if (sheet == null)
                {
                    sheet = SpriteSheet.Get("rainworld.default");
                }
                
                selectSpriteButtons[key].menuLabel.text = sheet.Name;
            }
        }

        public int GetCurrentlySelectedOfSeries(string series)
        {
            if (series.StartsWith("SLUGCAT_")) {
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
            if (infoLabel != null)
            {
                infoLabel.RemoveFromContainer();
            }
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "BACK")
            {
                manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                PlaySound(SoundID.MENU_Switch_Page_Out);

                SaveManager.Save();
            }

            else if (message.StartsWith("SPRITE_SELECTOR_"))
            {
                var spritename = message.Substring(16);

                var dialog = new GalleryDialog(spritename, manager, this);
                PlaySound(SoundID.MENU_Player_Join_Game);
                manager.ShowDialog(dialog);
            }
            else if (message == "RELOAD_ATLASES")
            {
                foreach (var sheet in Plugin.SpriteSheets)
                {
                    foreach (var atlas in sheet.Atlases)
                    {
                        Futile.atlasManager.UnloadAtlas(atlas.name);
                    }
                }

                Plugin.SpriteSheets.Clear();

                AtlasHooks.LoadAtlases();

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
            }

        public class GalleryDialog : Dialog, SelectOneButton.SelectOneButtonOwner
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

            public GalleryDialog(string spriteName, ProcessManager manager, FancyMenu owner)
                : base(manager)
            {
                this.owner = owner;
                this.spriteName = spriteName;

                border = new RoundedRect(this, pages[0], new Vector2(8, 42f), new Vector2(800, 725), true);

                darkSprite.anchorX = 0f;
                darkSprite.anchorY = 0f;
                darkSprite.scaleX = border.size.x - 12f;
                darkSprite.scaleY = border.size.y - 12f;
                darkSprite.x = border.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                darkSprite.y = border.pos.y + 6f;
                darkSprite.alpha = 1f;

                cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(110f, 30f));
                pages[0].subObjects.Add(cancelButton);

                spriteBoxes = new RoundedRect[4,3];

                var label = new MenuLabel(this, pages[0], spriteName, new Vector2((paddingX + darkSprite.x + darkSprite.scaleX - 200f) * 0.5f, (darkSprite.y + darkSprite.scaleY - 30f)), new Vector2(200f, 30f), true);
                pages[0].subObjects.Add(label);

                spriteSheets = new();
                foreach (var spriteSheet in Plugin.SpriteSheets)
                {
                    if (!spriteSheet.AvailableSprites.Any(x => x.Name == spriteName))
                    {
                        continue;
                    }

                    spriteSheets.Add(spriteSheet);
                }

                pageCount = ((spriteSheets.Count / (rows * columns)) + 1);

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

                SetupGallery();
                LoadPage(currentPageNumber);

                if (currentSelection < 0)
                {
                    currentSelection = 0;
                }

                var resetButton = new SimpleButton(this, pages[0], "RELOAD ATLASES", "RELOAD_ATLASES_GALLERY", owner.resetButton.pos, owner.resetButton.size);
                pages[0].subObjects.Add(resetButton);

                for (var i = 0; i < 4; i++)
                {
                    var ownerButton = owner.playerButtons[i];
                    var button = new SelectOneButton(this, pages[0], ownerButton.menuLabel.text, ownerButton.signalText, ownerButton.pos, ownerButton.size, playerButtons, i);
                    pages[0].subObjects.Add(button);
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
                        var pos = new Vector2(border.pos.x + paddingX + (boxMargin * x) + (boxSize * x), 768 - (paddingY + (boxMargin * y) + (boxSize * y) + (labelHeight * y) + boxSize));
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

                pageLabel.text = (page + 1) + "/" + pageCount;
                leftPage.inactive = page == 0;
                rightPage.inactive = page >= pageCount-1;

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
                            sprite.element = spriteSheet.TrimmedElements[SpriteDefinitions.AvailableSprites.Where(x => x.Name == spriteName).FirstOrDefault().GallerySprite];

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

                            if (Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex).CustomSprite(spriteName)?.SpriteSheetID == spriteSheet.ID)
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
                        var customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex);
                        var customSprite = customization.CustomSprite(spriteName);

                        if (customSprite == null)
                        {
                            customSprite = new()
                            {
                                Sprite = spriteName,
                                Enforce = true
                            };

                            customization.CustomSprites.Add(customSprite);
                        }

                        customSprite.SpriteSheetID = spriteSheets[(currentPageNumber * columns * rows) + to].ID;

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
                        if (sprite != null)
                        {
                            sprite.RemoveFromContainer();
                        }
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
        }
    }
}