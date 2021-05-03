﻿using System.Collections.Generic;
using Castle.Core.Internal;
using Godot;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    #if TOOLS
    [Tool]
    #endif
    public class ConstrainedManagedTextButton : ManagedTextButton, ITooltipComponent
    {
        public int PointRestriction { get; set; }
        
        public int Value { get; set; }

        public ICollection<string> Tooltip { get; set; }

        [Signal]
        public delegate void ValuePress(string myName, int delta, bool newValue);

        [Signal]
        public delegate void ValueToggle(string myName, int delta, bool newValue);

        protected override void Press()
        {
            if (this.PointRestriction < this.Value)
            {
                return;
            }
            
            base.Press();
            this.EmitSignal(
                "ValuePress",
                this.Name,
                this.Pressed ? 1 : -1,
                this.Pressed);

            if (this.ToggleMode)
            {
                this.EmitSignal(
                    "ValueToggle", 
                    this.Name,
                    this.Pressed ? 1 : -1,
                    this.Pressed);
            }
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            
            if (Input.IsActionPressed("tooltip_show") == false
                || this.Tooltip.IsNullOrEmpty())
            {
                return;
            }
            
            GlobalConstants.GameManager.GUIManager.Tooltip.Show(
                this.Name,
                null,
                this.Tooltip);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            
            GlobalConstants.GameManager.GUIManager.Tooltip.Hide();
        }
    }
}