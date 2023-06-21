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
using Menu.Remix.MixedUI;
using Menu.Remix;
using System.CodeDom;
using IL.MoreSlugcats;
using UnityEngine.UIElements;
using RWCustom;
using System.Security.Cryptography;
using UnityEngine.UI;

namespace DressMySlugcat
{
    public class FancyMenu : Dialog, SelectOneButton.SelectOneButtonOwner
    {
        public SimpleButton backButton;
        public SimpleButton resetButton;
        public RoundedRect textBoxBorder;
        public FSprite textBoxBack;
        public PlayerGraphicsDummy slugcatDummy;
        public Dictionary<string, SimpleButton> selectSpriteButtons = new();
        public Dictionary<string, MenuLabel> selectSpriteLabels = new();
        public Dictionary<string, SimpleButton> customizeSpriteButtons = new();
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

        public FancyMenu(ProcessManager manager, PauseMenu owner = null) : base(manager)
        {
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

            //playerButtons = new SelectOneButton[4];
            playerButtons = new SelectOneButton[this.manager.rainWorld.options.controls.Length]; //-WW -DYNAMIC LENGTH
            for (int i = 0; i < playerButtons.Length; i++)
            {
                var playerButton = new SelectOneButton(this, pages[0], "Player " + (i + 1), "PLAYER_" + i, textBoxBorder.pos + new Vector2(65 * i, -40), new Vector2(60, 30), playerButtons, i);
                pages[0].subObjects.Add(playerButton);
                playerButtons[i] = playerButton;
            }

            SetupSlugcatPages();
            LoadSlugcatPage(0);

            UpdateControls();

            resetButton = new SimpleButton(this, pages[0], "RELOAD ATLASES", "RELOAD_ATLASES", textBoxBorder.pos + new Vector2(textBoxBorder.size.x, 0) - new Vector2(160, 40), new Vector2(160f, 30f));
            pages[0].subObjects.Add(resetButton);
        }

        public override void Update()
        {
            base.Update();
            if (InGame)
            {
                if (RWInput.CheckPauseButton(0, owner.game.rainWorld))
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

            foreach (var button in customizeSpriteButtons.Values)
            {
                button.RemoveSprites();
                pages[0].RemoveSubObject(button);
            }
            customizeSpriteButtons.Clear();

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

                button = new SimpleButton(this, pages[0], "Customize", sprite.Name == "TAIL" ? "TAIL_CUSTOMIZER" : "SPRITE_CUSTOMIZER_" + sprite.Name, internalTopLeft + new Vector2(80 + 190, (i * -70) - 30), new Vector2(70f, 30f));
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
                var customSprite = customization.CustomSprite(key);
                SpriteSheet sheet = customSprite?.SpriteSheet;

                if (sheet == null)
                {
                    sheet = SpriteSheet.GetDefault();
                }

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
            if (infoLabel != null)
            {
                infoLabel.RemoveFromContainer();
            }
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
        }

        public class SpriteCustomizer : Dialog
        {
            public SimpleButton cancelButton;
            public SimpleButton resetButton;
            public RoundedRect border;
            FancyMenu owner;
            string sprite;
            SimpleButton selectedButton;

            public MenuTabWrapper tabWrapper;

            public Configurable<Color> colorConf;
            public OpColorPicker colorOp;

            public SpriteCustomizer(FancyMenu owner, string sprite) : base(owner.manager)
            {
                this.owner = owner;
                this.sprite = sprite;
                selectedButton = owner.customizeSpriteButtons[sprite];

                var pos = selectedButton.pos + new Vector2(selectedButton.size.x + 10, -120f);

                border = new RoundedRect(this, pages[0], pos, new Vector2(171, 207), true);

                darkSprite.anchorX = 0f;
                darkSprite.anchorY = 0f;
                darkSprite.scaleX = border.size.x - 12f;
                darkSprite.scaleY = border.size.y - 12f;
                darkSprite.x = border.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                darkSprite.y = border.pos.y + 6f;
                darkSprite.alpha = 1f;

                cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(50f, 30f));
                pages[0].subObjects.Add(cancelButton);

                resetButton = new SimpleButton(this, pages[0], "RESET", "RESET", new Vector2(darkSprite.x + darkSprite.scaleX - 5 - cancelButton.size.x, darkSprite.y + 5), cancelButton.size);
                pages[0].subObjects.Add(resetButton);

                var customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex);
                var customSprite = customization.CustomSprite(sprite, true);

                tabWrapper = new MenuTabWrapper(this, pages[0]);
                pages[0].subObjects.Add(tabWrapper);

                colorConf = new Configurable<Color>(default);
                colorOp = new OpColorPicker(colorConf, cancelButton.pos + new Vector2(0, 40));

                var onValueChanged = typeof(UIconfig).GetEvent("OnValueChanged");
                onValueChanged.AddEventHandler(colorOp, Delegate.CreateDelegate(onValueChanged.EventHandlerType, this, typeof(SpriteCustomizer).GetMethod("OnColorChanged")));

                new UIelementWrapper(tabWrapper, colorOp);

                colorOp.valueColor = customSprite.Color != default ? customSprite.Color : Utils.DefaultColorForSprite(owner.selectedSlugcat, sprite);
            }

            public void OnColorChanged(UIconfig sender, string oldValue, string newValue)
            {
                Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false).CustomSprite(sprite, true).Color = (sender as OpColorPicker).valueColor;
                owner.slugcatDummy.UpdateSprites();
            }

            public override void Singal(MenuObject sender, string message)
            {
                if (message == "BACK")
                {
                    Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false).CustomSprite(sprite, true).Color = colorOp.valueColor;

                    PlaySound(SoundID.MENU_Switch_Page_Out);
                    owner.slugcatDummy.UpdateSprites();
                    manager.StopSideProcess(this);
                }
                else if (message == "RESET")
                {
                    colorOp.valueColor = Utils.DefaultColorForSprite(owner.selectedSlugcat, sprite);
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                }
            }

            public override void Update()
            {
                base.Update();
                if (owner.InGame)
                {
                    if (RWInput.CheckPauseButton(0, owner.owner.game.rainWorld))
                    {
                        owner.Singal(owner.backObject, "BACK");
                        manager.StopSideProcess(this);
                    }
                }
            }
        }

        public class TailCustomizer : Dialog
        {
            public SimpleButton cancelButton;
            public SimpleButton resetButton;
            public RoundedRect border;
            public FancyMenu owner;
            public MenuTabWrapper tabWrapper;

            public Configurable<int> lengthConf;
            public Configurable<float> widenessConf;
            public Configurable<float> roundnessConf;
            public Configurable<int> offsetConf;
            public Configurable<Color> colorConf;
            public Configurable<bool> custTailShape; //-WW
            public Configurable<bool> asymTail;

            public OpSlider lengthOp;
            public OpFloatSlider widenessOp;
            public OpFloatSlider roundnessOp;
            public OpSlider offsetOp;
            public OpColorPicker colorOp;
            public OpCheckBox custTailOp; //-WW
            public OpCheckBox asymTailOp;

            public MenuLabel opMenuLbl1;
            public MenuLabel opMenuLbl2;
            public MenuLabel opMenuLbl3;
            public MenuLabel opMenuLbl4;
            public MenuLabel opTailLbl;
            public MenuLabel opAsymTailLbl;

            public Customization customization;

            //OLD MENU
            /*
            public TailCustomizer(FancyMenu owner) : base(owner.manager)
            {
                this.owner = owner;
                var tailButton = owner.customizeSpriteButtons["TAIL"];

                customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false);

                var pos = tailButton.pos + new Vector2(tailButton.size.x + 10, -100f);

                border = new RoundedRect(this, pages[0], pos, new Vector2(204, 320), true); //300

                darkSprite.anchorX = 0f;
                darkSprite.anchorY = 0f;
                darkSprite.scaleX = border.size.x - 12f;
                darkSprite.scaleY = border.size.y - 12f;
                darkSprite.x = border.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                darkSprite.y = border.pos.y + 6f;
                darkSprite.alpha = 1f;

                cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(50f, 30f));
                pages[0].subObjects.Add(cancelButton);

                resetButton = new SimpleButton(this, pages[0], "RESET", "RESET", new Vector2(darkSprite.x + darkSprite.scaleX - 5 - cancelButton.size.x, darkSprite.y + 5), cancelButton.size);
                pages[0].subObjects.Add(resetButton);

                tabWrapper = new MenuTabWrapper(this, pages[0]);
                pages[0].subObjects.Add(tabWrapper);

                #region madechangestosliders

                int btnY = 40;
                int btnPad = 65; //FROM 60 -WW
                int barLngt = 180;
                int lblYpad = 50; //FROM 40 -WW
                offsetConf = new Configurable<int>(6, new ConfigAcceptableRange<int>(2, 15));
                offsetOp = new OpSlider(offsetConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, offsetOp);
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], "Offset", offsetOp.pos + new Vector2(0, lblYpad), new Vector2(offsetOp.size.x, 20), true));

                btnY += btnPad;
                roundnessConf = new Configurable<float>(1, new ConfigAcceptableRange<float>(1, 3.6f));
                roundnessOp = new OpFloatSlider(roundnessConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, roundnessOp);
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], "Roundness", roundnessOp.pos + new Vector2(0, lblYpad), new Vector2(roundnessOp.size.x, 20), true));
                //Roundness is completely unused, but might be able to be repurposed for some extra sliders later

                btnY += btnPad;
                widenessConf = new Configurable<float>(8.6f, new ConfigAcceptableRange<float>(0.1f, 14.1f));
                widenessOp = new OpFloatSlider(widenessConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, widenessOp);
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], "Wideness", widenessOp.pos + new Vector2(0, lblYpad), new Vector2(widenessOp.size.x, 20), true));

                btnY += btnPad;
                lengthConf = new Configurable<int>(8, new ConfigAcceptableRange<int>(4, 17));
                lengthOp = new OpSlider(lengthConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, lengthOp);
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], "Length", lengthOp.pos + new Vector2(0, lblYpad), new Vector2(lengthOp.size.x, 20), true));

                #endregion

                custTailShape = new Configurable<bool>(false);
                Vector2 checkPos = new Vector2(cancelButton.pos.x + 14, border.pos.y + border.size.y + 12);
                custTailOp = new OpCheckBox(custTailShape, checkPos);
                new UIelementWrapper(tabWrapper, custTailOp);
                //            {
                //                description = dsc
                //            },
                //new OpLabel(45f, lineCount, BPTranslate("Allow Player-1 Character Change"))
                //            {
                //                description = dsc  //bumpBehav = chkBox5.bumpBehav, 
                //            }
                //pages[0].subObjects.Add(new OpLabel(custTailOp.pos + new Vector2(20, 0), lineCount, BPTranslate("Allow Player-1 Character Change")));
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], "Custom Tail Size", custTailOp.pos + new Vector2(10, 5), new Vector2(lengthOp.size.x, 20), true));
                


                colorConf = new Configurable<Color>(default);
                colorOp = new OpColorPicker(colorConf, new Vector2(resetButton.pos.x + resetButton.size.x + 14, border.pos.y));
                new UIelementWrapper(tabWrapper, colorOp);

                lengthOp.value = customization.CustomTail.Length.ToString();
                widenessOp.value = customization.CustomTail.Wideness.ToString();
                roundnessOp.value = customization.CustomTail.Roundness.ToString();
                offsetOp.value = customization.CustomTail.Lift.ToString();
                colorOp.valueColor = customization.CustomTail.Color != default ? customization.CustomTail.Color : Utils.DefaultBodyColor(owner.selectedSlugcat);
                custTailOp.value = customization.CustomTail.CustTailShape.ToString().ToLower(); //-WW 
                //custTailOp.
                Debug.LogFormat("IS CUSTOM TAIL?: " + custTailOp.value);

                UpdateSliderVis(); //-WW
            }
            */



            public TailCustomizer(FancyMenu owner) : base(owner.manager)
            {
                this.owner = owner;
                var tailButton = owner.customizeSpriteButtons["TAIL"];

                customization = Customization.For(owner.selectedSlugcat, owner.selectedPlayerIndex, false);

                var pos = tailButton.pos + new Vector2(tailButton.size.x + 10, -100f);

                border = new RoundedRect(this, pages[0], pos, new Vector2(204, 335), true); //300

                darkSprite.anchorX = 0f;
                darkSprite.anchorY = 0f;
                darkSprite.scaleX = border.size.x - 12f;
                darkSprite.scaleY = border.size.y - 12f;
                darkSprite.x = border.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                darkSprite.y = border.pos.y + 6f;
                darkSprite.alpha = 1f;

                cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(50f, 30f));
                pages[0].subObjects.Add(cancelButton);

                resetButton = new SimpleButton(this, pages[0], "RESET", "RESET", new Vector2(darkSprite.x + darkSprite.scaleX - 5 - cancelButton.size.x, darkSprite.y + 5), cancelButton.size);
                pages[0].subObjects.Add(resetButton);

                tabWrapper = new MenuTabWrapper(this, pages[0]);
                pages[0].subObjects.Add(tabWrapper);

                #region madechangestosliders

                int btnY = 40;
                int btnPad = 65; //FROM 60 -WW
                int barLngt = 180;
                int lblYpad = 50; //FROM 40 -WW
                offsetConf = new Configurable<int>(6, new ConfigAcceptableRange<int>(2, 15));
                /*
                offsetOp = new OpSlider(offsetConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, offsetOp);
                pages[0].subObjects.Add(opMenuLbl1 = new MenuLabel(this, pages[0], "Offset", offsetOp.pos + new Vector2(0, lblYpad), new Vector2(offsetOp.size.x, 20), true));
                //THIS WAS MOON'S NEW SLIDER, BUT UNTIL THE FORMULAS ARE FIXED, IT IS STAYING UNUSED
                offsetOp.Hidden = true;
                offsetOp.Hide();
                opMenuLbl1.label.isVisible = false;
                offsetOp._rect.isHidden = true;
                offsetOp._rect.Hide();
                offsetOp._label.isVisible = false;
                for (int i = 0; i < offsetOp._lineSprites.Length; i++)
                {
                    offsetOp._lineSprites[i].isVisible = false;
                }
                */
                //GOOD LORD OKAY YOU WIN. I CAN'T MAKE IT DISSAPEAR. I'LL JUST DESTROY IT

                //btnY += btnPad;
                roundnessConf = new Configurable<float>(0.1f, new ConfigAcceptableRange<float>(0.1f, 1.5f));
                roundnessOp = new OpFloatSlider(roundnessConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, roundnessOp);
                pages[0].subObjects.Add(opMenuLbl2 =new MenuLabel(this, pages[0], "Roundness", roundnessOp.pos + new Vector2(0, lblYpad), new Vector2(roundnessOp.size.x, 20), true));

                btnY += btnPad;
                widenessConf = new Configurable<float>(1f, new ConfigAcceptableRange<float>(0.1f, 10f));
                widenessOp = new OpFloatSlider(widenessConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, widenessOp);
                pages[0].subObjects.Add(opMenuLbl3 = new MenuLabel(this, pages[0], "Wideness", widenessOp.pos + new Vector2(0, lblYpad), new Vector2(widenessOp.size.x, 20), true));

                btnY += btnPad;
                lengthConf = new Configurable<int>(4, new ConfigAcceptableRange<int>(2, 15));
                lengthOp = new OpSlider(lengthConf, cancelButton.pos + new Vector2(5, btnY), barLngt);
                new UIelementWrapper(tabWrapper, lengthOp);
                pages[0].subObjects.Add(opMenuLbl4 = new MenuLabel(this, pages[0], "Length", lengthOp.pos + new Vector2(0, lblYpad), new Vector2(lengthOp.size.x, 20), true));

                #endregion

                btnY += btnPad + 15;
                custTailShape = new Configurable<bool>(false);
                Vector2 checkPos = new Vector2(cancelButton.pos.x + 0, cancelButton.pos.y + btnY); //border.pos.y + border.size.y - 35
                custTailOp = new OpCheckBox(custTailShape, checkPos);
                new UIelementWrapper(tabWrapper, custTailOp);
                pages[0].subObjects.Add(opTailLbl = new MenuLabel(this, pages[0], "Custom Tail Size", custTailOp.pos + new Vector2(15, 5), new Vector2(lengthOp.size.x, 20), true));


                //-FB tail asymmetry option
                btnY += (btnPad / 3) + 20;
                asymTail = new Configurable<bool>(false);
                var asymCheckPos = new Vector2(cancelButton.pos.x + 0, cancelButton.pos.y + btnY);
                asymTailOp = new OpCheckBox(asymTail, asymCheckPos);
                new UIelementWrapper(tabWrapper, asymTailOp);
                pages[0].subObjects.Add(opAsymTailLbl = new MenuLabel(this, pages[0], "Asymmetry", asymTailOp.pos + new Vector2(15, 5), new Vector2(lengthOp.size.x, 20), true));


                //-WW -CHECK IF WE ARE A CUSTOM SLUGCAT WITH CUSTOM DEFAULT TAIL VALUES WE CAN RESET TO
                bool lockTailSize = false;
                var defaults = SpriteDefinitions.GetSlugcatDefault(customization.Slugcat, customization.PlayerNumber)?.Copy();
                if (defaults != null)
                {
                    if (defaults.CustomTail.ForbidTailResize)
                    {
                        lockTailSize = true;
                        customization.CustomTail.CustTailShape = false;
                    }

                    if (defaults.CustomTail.Length == 0)
                        Debug.LogFormat("DEFAULT NOT PROVIDED: ");

                    //-WW - IF OUR TEMPLATE DIDN'T PROVIDE THESE VALUES, DEFAULT TO SURVIVORS
                    if (defaults.CustomTail.Length == 0)
                        customization.CustomTail.Length = 4;
                    if (defaults.CustomTail.Wideness == 0)
                        customization.CustomTail.Wideness = 1;
                    if (defaults.CustomTail.Roundness == 0)
                        customization.CustomTail.Roundness = 0.1f;
                }
                    
                

                //Debug.LogFormat("IS CUSTOM TAIL?: " + customization.CustomTail.ForbidTailResize);
                //THAT'S IT. DON'T LET THEM CHANGE THIS IN-GAME
                if (owner.InGame || lockTailSize)
                {
                    custTailOp.greyedOut = true;
                    opTailLbl.label.alpha = 0.5f;
                    string msgWarn = "(Change from main menu)";
                    if (lockTailSize) //ALLOW SLUGCAT CONFIGS TO PREVENT CUSTOM TAIL SHAPES
                        msgWarn = "(Not available for this slugcat)";
                    pages[0].subObjects.Add(new MenuLabel(this, pages[0], msgWarn, custTailOp.pos + new Vector2(13, 45 * 0.45f), new Vector2(lengthOp.size.x, 20), false));
                }
                else
                {
                    opTailLbl.label.alpha = 1f;
                }
                


                colorConf = new Configurable<Color>(default);
                colorOp = new OpColorPicker(colorConf, new Vector2(resetButton.pos.x + resetButton.size.x + 14, border.pos.y));
                new UIelementWrapper(tabWrapper, colorOp);

                lengthOp.value = customization.CustomTail.Length.ToString();
                widenessOp.value = customization.CustomTail.Wideness.ToString();
                roundnessOp.value = customization.CustomTail.Roundness.ToString();
                //offsetOp.value = customization.CustomTail.Lift.ToString();
                colorOp.valueColor = customization.CustomTail.Color != default ? customization.CustomTail.Color : Utils.DefaultBodyColor(owner.selectedSlugcat);
                custTailOp.value = customization.CustomTail.CustTailShape.ToString().ToLower(); //-WW  THIS HAS TO BE LOWERCASE
                asymTailOp.value = customization.CustomTail.AsymTail.ToString().ToLower(); //-FB noted lol
                //Debug.LogFormat("IS CUSTOM TAIL?: " + custTailOp.value);

                //CATCH ANY OUTDATED DEFAULT SETUPS AND RESET THEM 
                if (customization.CustomTail.Length.ToString() == "0" && customization.CustomTail.Wideness.ToString() == "0")
                {
                    Debug.LogFormat("LEGACY DEFAULT VALUES DETECTED ");
                    Singal(owner.backObject, "RESET");
                    //Singal(owner.backObject, "RESET");
                }
                //SECOND PASS TO CLEAN UP LEGACY CUSTOM TAIL VALUES. (we can only really check Length, because all legacy wideness values are legal. and roundness is the same)
                if (customization.CustomTail.Length <= 1)
                {
                    Debug.LogFormat("LEGACY TAIL LENGTH DETECTED ");
                    lengthOp.value = Mathf.Lerp(2, 15, customization.CustomTail.Length).ToString();
                    widenessOp.value = (customization.CustomTail.Wideness / 10f).ToString();
                }

                UpdateSliderVis(); //-WW
            }



            public override void Singal(MenuObject sender, string message)
            {
                if (message == "BACK")
                {
                    customization.CustomTail.Length = int.Parse(lengthOp.value);
                    customization.CustomTail.Wideness = float.Parse(widenessOp.value);
                    customization.CustomTail.Roundness = float.Parse(roundnessOp.value);
                    customization.CustomTail.Lift = 0; // int.Parse(offsetOp.value);
                    customization.CustomTail.Color = colorOp.valueColor;
                    customization.CustomTail.CustTailShape = bool.Parse(custTailOp.value); //-WW 
                    customization.CustomTail.AsymTail = bool.Parse(asymTailOp.value);
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                    manager.StopSideProcess(this);
                }
                else if (message == "RESET")
                {
                    //Debug.LogFormat("RESET ");
                    lengthOp.value = "4"; //-WW - DEFAULTS ACTUALLY GO TO A NORMAL TAIL VALUE
                    widenessOp.value = "1";
                    roundnessOp.value = "0.1";
                    //offsetOp.value = "0";
                    colorOp.valueColor = Utils.DefaultBodyColor(owner.selectedSlugcat);

                    //-WW -CHECK IF WE ARE A CUSTOM SLUGCAT WITH CUSTOM DEFAULT TAIL VALUES WE CAN RESET TO
                    var defaults = SpriteDefinitions.GetSlugcatDefault(customization.Slugcat, customization.PlayerNumber)?.Copy();
                    if (defaults != null)
                    {
                        lengthOp.value = defaults.CustomTail.Length.ToString();
                        widenessOp.value = defaults.CustomTail.Wideness.ToString();
                        roundnessOp.value = defaults.CustomTail.Roundness.ToString();
                        //offsetOp.value = defaults.CustomTail.Lift.ToString();
                    }

                    PlaySound(SoundID.MENU_Switch_Page_Out);
                }
            }

            //-WW
            public void UpdateSliderVis()
            {
                if (custTailOp != null)
                {
                    //Debug.LogFormat("UPDATING SLIDER VISUALS!: " + custTailOp.value);
                    if (custTailOp.value.ToLower() == "false")
                    {
                        //offsetOp.greyedOut = true;
                        lengthOp.greyedOut = true;
                        widenessOp.greyedOut = true;
                        roundnessOp.greyedOut = true;
                        //opMenuLbl1.label.isVisible = false;
                        lengthOp._rect.Hide();
                        widenessOp._rect.Hide();
                        roundnessOp._rect.Hide();
                        opMenuLbl2.label.isVisible = false;
                        opMenuLbl3.label.isVisible = false;
                        opMenuLbl4.label.isVisible = false;
                        owner.slugcatDummy.DefaultDummyTailDisplay(); //REVERT THE TAIL PREVIEW TO IT'S DEFAULT SIZE
                    }
                    else
                    {
                        //offsetOp.greyedOut = false;
                        lengthOp.greyedOut = false;
                        widenessOp.greyedOut = false;
                        roundnessOp.greyedOut = false;
                        //opMenuLbl1.label.isVisible = true; //NOT THIS ONE
                        lengthOp._rect.Show();
                        widenessOp._rect.Show();
                        roundnessOp._rect.Show();
                        opMenuLbl2.label.isVisible = true;
                        opMenuLbl3.label.isVisible = true;
                        opMenuLbl4.label.isVisible = true;
                        UpdateDummyTailDisplay(); //UPDATE THE PREVIEW BASED ON SLIDERS
                    }

                }
            }


            public void UpdateDummyTailDisplay()
            {
                //THIS DOESN'T RUN IF CUSTOM IS SET TO FALSE
                int length = int.Parse(lengthOp.value);
                float wideness = float.Parse(widenessOp.value);
                float roundness = float.Parse(roundnessOp.value);
                int offset = 0; // int.Parse(offsetOp.value);
                bool pup = false;
                
                for (var i = 0; i < owner.slugcatDummy.TailSprites.Length; i++)
                {
                    if (i > int.Parse(lengthOp.value) - 1)
                    {
                        owner.slugcatDummy.TailSprites[i].isVisible = false;
                    }
                    else
                    {
                        owner.slugcatDummy.TailSprites[i].isVisible = true;
                        float radX = PlayerGraphicsHooks.GetSegmentRadius(i, length, wideness, roundness, offset, pup);
                        owner.slugcatDummy.TailSprites[i].scaleX = PlayerGraphicsDummy.tailXScale * radX;
                    }
                }
            }


            public override void Update()
            {
                base.Update();
                if (owner.InGame)
                {
                    if (RWInput.CheckPauseButton(0, owner.owner.game.rainWorld))
                    {
                        owner.Singal(owner.backObject, "BACK");
                        manager.StopSideProcess(this);
                    }
                }

                UpdateSliderVis();
            }
        }

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
                darkSprite.x = border.pos.x + 6f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
                darkSprite.y = border.pos.y + 6f;
                darkSprite.alpha = 1f;

                cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(110f, 30f));
                pages[0].subObjects.Add(cancelButton);

                spriteBoxes = new RoundedRect[4, 3];

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
                    if (RWInput.CheckPauseButton(0, owner.owner.game.rainWorld))
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
                                sprite.element = defaulSpritesheet.TrimmedElements[SpriteDefinitions.AvailableSprites.Where(x => x.Name == spriteName).FirstOrDefault().GallerySprite];
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

            public bool GetChecked(CheckBox box)
            {
                switch (box.IDString)
                {
                    case "USE_DEFAULTS":
                        return owner.useDefaults;
                    case "USE_ENTIRE_SET":
                        return owner.useEntireSet;
                    default:
                        return false;
                }
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
    }
}