using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
{
    #if TOOLS
    [Tool]
    #endif
    public class IntValueItem : Control
    {
        protected ManagedLabel NameLabel { get; set; }
        protected ManagedLabel ValueLabel { get; set; }
        
        public int Maximum { get; set; }
        public int Minimum { get; set; }

        public string ValueName
        {
            get =>this.NameLabel?.Text;
            set
            {
                if (this.NameLabel is null)
                {
                    GD.Print(this.GetType().Name + " NameLabel is null!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }

                    return;
                }
                
                this.NameLabel.Text = value;
            }
        }

        public int Value
        {
            get => int.TryParse(this.ValueLabel.Text, out int value) ? value : 0;
            set
            {
                if (this.ValueLabel is null)
                {
                    GD.Print(this.GetType().Name + " ValueLabel is null!");
                    if (this.IsInsideTree())
                    {
                        GD.Print(this.GetPath());
                    }

                    return;
                }
                
                this.ValueLabel.Text = value.ToString();
            }
        }

        public override void _Ready()
        {
            base._Ready();
            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            this.NameLabel = this.GetNodeOrNull("Item Name") as ManagedLabel;
        }

        public void IncreaseValue(int delta = 1)
        {
            this.Value = Mathf.Clamp(this.Value + delta, this.Minimum, this.Maximum);
        }

        public void DecreaseValue(int delta = 1)
        {
            this.Value = Mathf.Clamp(this.Value - delta, this.Minimum, this.Maximum);
        }
    }
}