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
    public class StringValueItem : Control, IValueItem<string>
    {
        protected ManagedLabel NameLabel { get; set; }
        protected ManagedLabel ValueLabel { get; set; }

        public ICollection<string> Values
        {
            get => this.m_Values;
            set
            {
                this.m_Values = value;
                this.Minimum = 0;
                this.Maximum = this.m_Values.Count;
            }
        }

        protected ICollection<string> m_Values;
        
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        protected int Index { get; set; }

        [Signal]
        public delegate void ValueChanged(int delta, int newValue);

        public string ValueName
        {
            get => this.NameLabel?.Text;
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
                this.Name = value;
            }
        }

        public string Value
        {
            get => this.ValueLabel?.Text;
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
                
                this.ValueLabel.Text = value;
            }
        }

        public override void _Ready()
        {
            base._Ready();
            GD.Print(this.GetType().Name + " initialising");
            this.Values = new List<string>();
            this.ValueLabel = this.GetNodeOrNull("Item Value") as ManagedLabel;
            if (this.ValueLabel is null)
            {
                GD.Print("ValueLabel of " + this.GetType().Name + " is null!");
                if (this.IsInsideTree())
                {
                    GD.Print(this.GetPath());
                }
            }

            this.NameLabel = this.GetNodeOrNull("Item Name") as ManagedLabel;
            if (this.NameLabel is null)
            {
                GD.Print("NameLabel of " + this.GetType().Name + " is null!");
                if (this.IsInsideTree())
                {
                    GD.Print(this.GetPath());
                }
            }
            GD.Print(this.GetType().Name + " finished initialising");
        }

        public void ChangeValue(int delta = 1)
        {
            GD.Print("Calling " + nameof(this.ChangeValue));
            this.Index += delta;
            this.Index %= this.Values.Count > 0 ? this.Values.Count : 1;
            this.Index = Math.Abs(this.Index);
            
            if (this.Index < this.Values.Count)
            {
                this.ValueLabel.Text = this.Values.ElementAt(this.Index);
            }
            
            this.EmitSignal("ValueChanged", delta, this.Value);
        }
    }
}