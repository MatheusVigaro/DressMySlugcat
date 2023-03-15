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
using IL.Menu.Remix;

namespace DressMySlugcat
{
    public class FancyMenu : Menu.Menu
    {
        public FSprite darkSprite;
        public SimpleButton backButton;
        public RoundedRect textBoxBorder;
        public FSprite textBoxBack;
        public PlayerGraphicsDummy slugcatDummy;

        public FancyMenu(ProcessManager manager) : base (manager, Plugin.FancyMenu)
        {

            pages.Add(new Page(this, null, "main", 0));
            scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
            pages[0].subObjects.Add(scene);

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
            textBoxBorder = new RoundedRect(this, pages[0], new Vector2(255f, 50f), new Vector2(1050f, 700f), filled: true);

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

            for (var i = 0; i < SlugcatStats.Name.values.entries.Count; i++)
            {
                var slugcatName = SlugcatStats.Name.values.entries[i];
                var slugcatButton = new SimpleButton(this, pages[0], slugcatName, "SLUGCAT_" + slugcatName, new Vector2(15f, 768f - (50f + (35f * i))), new Vector2(220f, 30f));
                pages[0].subObjects.Add(slugcatButton);
            }

            var spriteSelectors = new string[] { "Head", "Face", "Body", "Arms", "Hips", "Legs", "Tail" };
            SpriteParts["HEAD"] = new int[] { 3 };
            SpriteParts["FACE"] = new int[] { 9 };
            SpriteParts["BODY"] = new int[] { 0 };
            SpriteParts["ARMS"] = new int[] { 5, 6 };
            SpriteParts["HIPS"] = new int[] { 1 };
            SpriteParts["LEGS"] = new int[] { 4 };
            SpriteParts["TAIL"] = new int[] { 2 };

            var internalTopLeft = new Vector2(190f, 700f);
            for (var i = 0; i < spriteSelectors.Length; i++) {
                pages[0].subObjects.Add(new MenuLabel(this, pages[0], spriteSelectors[i], internalTopLeft + new Vector2(72, i * -70f), new Vector2(200f, 30f), bigText: true));
                pages[0].subObjects.Add(new SimpleButton(this, pages[0], spriteSelectors[i], "SPRITE_SELECTOR_" + spriteSelectors[i].ToUpper(), internalTopLeft + new Vector2(80, (i * -70) - 30), new Vector2(180f, 30f)));
            }

            slugcatDummy = new PlayerGraphicsDummy();
            slugcatDummy.SlugcatPosition = new Vector2(133f, 70f);
            slugcatDummy.Container.scale = 8f;

            mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        }

        Dictionary<string, int[]> SpriteParts = new();

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
            }
        }
    }
}