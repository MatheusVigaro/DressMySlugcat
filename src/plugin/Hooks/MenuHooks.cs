namespace DressMySlugcat.Hooks;

public class MenuHooks
{
    public static void Init()
    {
        On.Menu.MainMenu.ctor += MainMenu_ctor;
        IL.Menu.MainMenu.AddMainMenuButton += MainMenu_AddMainMenuButton;
        IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
    }

    private static void ProcessManager_PostSwitchMainProcess(ILContext il)
    {
        var cursor = new ILCursor(il);

        try
        {
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                     i => i.MatchLdfld<ProcessManager>(nameof(ProcessManager.oldProcess)),
                                                     i => i.MatchLdarg(0),
                                                     i => i.MatchLdfld<ProcessManager>(nameof(ProcessManager.currentMainLoop)),
                                                     i => i.MatchCallOrCallvirt<MainLoopProcess>(nameof(MainLoopProcess.CommunicateWithUpcomingProcess))))
            {
                throw new Exception("Failed to match IL for ProcessManager_PostSwitchMainProcess!");
            }
        }
        catch (Exception ex)
        {
            DebugError("Exception when matching IL for ProcessManager_PostSwitchMainProcess!");
            DebugError(ex);
            DebugError(il);
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
        {
            if (ID == Plugin.FancyMenu)
            {
                self.currentMainLoop = new FancyMenu(self);
            }
        });

    }

    private static void MainMenu_AddMainMenuButton(ILContext il)
    {
        var cursor = new ILCursor(il);

        try
        {
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(8)))
            {
                throw new Exception("Failed to match IL for MainMenu_ctor1!");
            }

            cursor.MoveAfterLabels();
            cursor.EmitDelegate((int _) => 12);
        }
        catch (Exception ex)
        {
            DebugError("Exception when matching IL for MainMenu_ctor1!");
            DebugError(ex);
            DebugError(il);
            throw;
        }
    }

    private static void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
    {
        orig(self, manager, showRegionSpecificBkg);

        float buttonWidth = Menu.MainMenu.GetButtonWidth(self.CurrLang);
        Vector2 pos = new(683f - (buttonWidth / 2f), 0f);
        Vector2 size = new(buttonWidth, 30f);

        self.AddMainMenuButton(new SimpleButton(self, self.pages[0], "GET FANCY", "GETFANCY", pos, size), () =>
        {
            manager.RequestMainProcessSwitch(Plugin.FancyMenu);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }, 50);
    }
}