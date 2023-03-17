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

namespace DressMySlugcat
{
    public class FancyMenu : Menu.Menu
    {
        public FSprite darkSprite;
        public SimpleButton backButton;
        public RoundedRect textBoxBorder;
        public FSprite textBoxBack;
        public PlayerGraphicsDummy slugcatDummy;
        public Dictionary<string, SpriteSheet> selectedSprites;

        public FancyMenu(ProcessManager manager) : base (manager, Plugin.FancyMenu)
        {
            selectedSprites = new();

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

            for (var i = 0; i < SlugcatStats.Name.values.entries.Count; i++)
            {
                var slugcatName = SlugcatStats.Name.values.entries[i];
                var slugcatDisplayName = slugcatName;
                SlugcatStats.Name.TryParse(typeof(SlugcatStats.Name), slugcatName, true, out var slugcatNameExtEnum);
                if (slugcatNameExtEnum != null)
                {
                    slugcatDisplayName = SlugcatStats.getSlugcatName((SlugcatStats.Name)slugcatNameExtEnum);
                }

                var slugcatButton = new SimpleButton(this, pages[0], slugcatDisplayName, "SLUGCAT_" + slugcatName, new Vector2(15f, 768f - (50f + (35f * i))), new Vector2(220f, 30f));
                pages[0].subObjects.Add(slugcatButton);
            }

            var spriteSelectors = new string[] { "Head", "Face", "Body", "Arms", "Hips", "Legs", "Tail" };

            var internalTopLeft = new Vector2(190f, 700f);

            for (var i = 0; i < spriteSelectors.Length; i++)
            {
                var label = new MenuLabel(this, pages[0], spriteSelectors[i], internalTopLeft + new Vector2(72, i * -70f), new Vector2(200f, 30f), bigText: true);
                pages[0].subObjects.Add(label);

                var button = new SimpleButton(this, pages[0], spriteSelectors[i], "SPRITE_SELECTOR_" + spriteSelectors[i].ToUpper(), internalTopLeft + new Vector2(80, (i * -70) - 30), new Vector2(180f, 30f));
                pages[0].subObjects.Add(button);
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
            }
            else if (message.StartsWith("SLUGCAT_"))
            {
                var slugcatName = message.Substring(8);
            }
            else if (message.StartsWith("SPRITE_SELECTOR_"))
            {
                var spritename = message.Substring(16);

                var dialog = new GalleryDialog(spritename, manager, this);
                PlaySound(SoundID.MENU_Player_Join_Game);
                manager.ShowDialog(dialog);

                /*
                for (int sprite = 0; sprite < SpriteParts[spritename].Length; sprite++)
                {
                    var i = SpriteParts[spritename][sprite];
                    if (slugcatDummy.Sprites[i].element.name.StartsWith(Plugin.BaseName + "_dressmyslugcat.saintshirt_"))
                    {
                        slugcatDummy.Sprites[i].element = Futile.atlasManager.GetElementWithName(slugcatDummy.Sprites[i].element.name.Substring((Plugin.BaseName + "_dressmyslugcat.saintshirt_").Length));
                    }
                    else
                    {
                        slugcatDummy.Sprites[i].element = Futile.atlasManager.GetElementWithName(Plugin.BaseName + "_dressmyslugcat.saintshirt_" + slugcatDummy.Sprites[i].element.name);
                    }
                }
                RemoveMainView();*/
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
            public int currentSelection;
            public FancyMenu owner;
            public string spriteName;

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

                cancelButton = new SimpleButton(this, pages[0], "BACK", "CANCEL", new Vector2(darkSprite.x + 5, darkSprite.y + 5), new Vector2(110f, 30f));
                pages[0].subObjects.Add(cancelButton);

                spriteBoxes = new RoundedRect[4,3];
                var paddingX = 18;
                var paddingY = 70;
                var boxMargin = 15;
                var boxSize = 180;
                var labelHeight = 20;

                var label = new MenuLabel(this, pages[0], spriteName, new Vector2((paddingX + darkSprite.x + darkSprite.scaleX - 200f) * 0.5f, (darkSprite.y + darkSprite.scaleY - 30f)), new Vector2(200f, 30f), true);
                pages[0].subObjects.Add(label);

                spriteSheets = new();
                foreach (var spriteSheet in Plugin.SpriteSheets)
                {
                    if (!spriteSheet.AvailableSprites.Contains(spriteName))
                    {
                        continue;
                    }

                    spriteSheets.Add(spriteSheet);
                }

                galleryButtons = new SelectOneButton[spriteSheets.Count];
                gallerySprites = new FSprite[spriteSheets.Count];
                galleryLabels = new MenuLabel[spriteSheets.Count];

                for (var y = 0; y < 3; y++)
                {
                    for (var x = 0; x < 4; x++)
                    {
                        var n = (y * 4) + x;
                        if (n < spriteSheets.Count)
                        {
                            var spriteSheet = spriteSheets[n];
                            var pos = new Vector2(border.pos.x + paddingX + (boxMargin * x) + (boxSize * x), 768 - (paddingY + (boxMargin * y) + (boxSize * y) + (labelHeight * y) + boxSize));
                            var size = new Vector2(boxSize, boxSize);

                            galleryLabels[n] = new MenuLabel(this, pages[0], spriteSheet.Name, pos + new Vector2(0, size.y+5), new Vector2(size.x, 20f), true);
                            pages[0].subObjects.Add(galleryLabels[n]);

                            switch (spriteName)
                            {
                                case "HEAD":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["HeadA0"]);
                                    break;
                                case "FACE":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["FaceA0"]);
                                    break;
                                case "BODY":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["BodyA"]);
                                    break;
                                case "ARMS":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["PlayerArm12"]);
                                    break;
                                case "HIPS":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["HipsA"]);
                                    gallerySprites[n].scaleY = -1;
                                    break;
                                case "LEGS":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["LegsA0"]);
                                    break;
                                case "TAIL":
                                    gallerySprites[n] = new FSprite(spriteSheet.TrimmedElements["TailTexture"]);
                                    break;
                            }

                            var sprite = gallerySprites[n];
                            container.AddChild(sprite);

                            sprite.x = pos.x + 1 + ((size.x - 4) / 2);
                            sprite.y = pos.y + 1 + ((size.x - 4) / 2);
                            sprite.anchorX = 0.5f; 
                            sprite.anchorY = 0.5f;

                            var element = sprite.element;
                            if (element.sourceSize.x > element.sourceSize.y)
                            {
                                var targetSize = size.x - 4f;
                                var scale = targetSize / element.sourceSize.x;

                                sprite.scaleX *= scale;
                                sprite.scaleY *= scale;
                                var ySize = element.sourceSize.x * scale;
                                //sprite.y = sprite.y + (targetSize - ySize) / 2;
                            }
                            else
                            {
                                var targetSize = size.y - 4f;
                                var scale = targetSize / element.sourceSize.y;

                                sprite.scaleX *= scale;
                                sprite.scaleY *= scale;
                                var xSize = element.sourceSize.y * scale;
                                //sprite.x = sprite.x + (targetSize - xSize) / 2;
                            }

                            galleryButtons[n] = new SelectOneButton(this, pages[0], "", "SELECTED_" + spriteSheet.ID, pos, size, galleryButtons, n);
                            pages[0].subObjects.Add(galleryButtons[n]);
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

                return 0;
            }

            public void SetCurrentlySelectedOfSeries(string series, int to)
            {
                if (series.StartsWith("SELECTED_") && currentSelection != to)
                {
                    currentSelection = to;
                    owner.selectedSprites[spriteName] = spriteSheets[to];
                    owner.slugcatDummy.UpdateSprites();
                }
            }

            public override void Singal(MenuObject sender, string message)
            {
                if (message != null && message == "CANCEL")
                {
                    foreach (var sprite in gallerySprites)
                    {
                        sprite.RemoveFromContainer();
                    }

                    manager.StopSideProcess(this);
                }
            }
        }
    }
}