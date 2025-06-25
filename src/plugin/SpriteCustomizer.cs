namespace DressMySlugcat;

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

        var pos = selectedButton.pos + new Vector2(selectedButton.size.x + 10 - owner.leftAnchor, -120f);
        pos.y = Mathf.Max(55, pos.y); //DON'T LET THIS APPEAR UNDERNEATH THE SELECTABLE WINDOW

        border = new RoundedRect(this, pages[0], pos, new Vector2(171, 207), true);

        darkSprite.anchorX = 0f;
        darkSprite.anchorY = 0f;
        darkSprite.scaleX = border.size.x - 12f;
        darkSprite.scaleY = border.size.y - 12f;
        darkSprite.x = border.pos.x + 6f;
        darkSprite.y = border.pos.y + 6f;
        darkSprite.alpha = 1f;

        cancelButton = new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(darkSprite.x + 5 + owner.leftAnchor, darkSprite.y + 5), new Vector2(50f, 30f));
        pages[0].subObjects.Add(cancelButton);

        resetButton = new SimpleButton(this, pages[0], "RESET", "RESET", new Vector2(darkSprite.x + darkSprite.scaleX - 5 - cancelButton.size.x + owner.leftAnchor, darkSprite.y + 5), cancelButton.size);
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
            if (RWInput.CheckPauseButton(0))
            {
                owner.Singal(owner.backObject, "BACK");
                manager.StopSideProcess(this);
            }
        }
    }
}
