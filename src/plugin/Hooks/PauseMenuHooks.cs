namespace DressMySlugcat.Hooks
{
    public class PauseMenuHooks
    {
        public static void Init()
        {
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
            On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        }

        private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);

            var fancyButton = new SimpleButton(self, self.pages[0], "GET FANCY", "GETFANCY", new Vector2(self.continueButton.pos.x, self.continueButton.pos.y + 38f), new Vector2(110f, 30f));
            self.pages[0].subObjects.Add(fancyButton);
            //WW - Set up controller navigation for the menu buttons
            fancyButton.nextSelectable[3] = self.continueButton;
            self.continueButton.nextSelectable[1] = fancyButton;
        }

        private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, PauseMenu self, MenuObject sender, string message)
        {
            orig(self, sender, message);

            if (message == "GETFANCY")
            {
                self.manager.ShowDialog(new FancyMenu(self.manager, self));
                self.container.alpha = 0;
            }
        }
    }
}