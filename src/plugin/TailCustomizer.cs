using DressMySlugcat.Hooks;

namespace DressMySlugcat;

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

        var pos = tailButton.pos + new Vector2(tailButton.size.x + 10 - owner.leftAnchor, -100f);

        border = new RoundedRect(this, pages[0], pos, new Vector2(204, 335), true); //300

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
        pages[0].subObjects.Add(opMenuLbl2 = new MenuLabel(this, pages[0], "Roundness", roundnessOp.pos + new Vector2(0, lblYpad), new Vector2(roundnessOp.size.x, 20), true));

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
        Vector2 checkPos = new(cancelButton.pos.x + 0, cancelButton.pos.y + btnY); //border.pos.y + border.size.y - 35
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
                DebugLog("DEFAULT NOT PROVIDED: ");

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

            owner.slugcatDummy.UpdateSprites(); //-FB fix the tail colour not updating

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
            if (RWInput.CheckPauseButton(0))
            {
                owner.Singal(owner.backObject, "BACK");
                manager.StopSideProcess(this);
            }
        }

        UpdateSliderVis();
    }
}
