using System;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Settings;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI
{
    public class GUIData : Control
    {
        protected IGUIManager m_GUIManager;

        public int DefaultSortingOrder { get; protected set; }

        [Export] public bool RemovesControl { get; protected set; }

        [Export] public bool ClosesOthers { get; protected set; }

        [Export] public bool AlwaysOpen { get; protected set; }

        [Export] public bool AlwaysOnTop { get; protected set; }

        protected IEntity Player { get; set; }

        protected bool EnableHappiness { get; set; }

        public virtual event GUIClosedEventHandler OnGUIClose;
        public virtual event GUIOpenedEventHandler OnGUIOpen;

        public IGUIManager GUIManager
        {
            get => this.m_GUIManager; /*?? (this.m_GUIManager = GlobalConstants.GameManager.GUIManager)*/
            set
            {
                this.m_GUIManager = value;
                Array children = this.GetChildren();
                foreach (var child in children)
                {
                    if (child is GUIData data)
                    {
                        data.GUIManager = value;
                    }
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (this.Visible == false)
            {
                return;
            }

            base._Input(@event);

            if (@event.IsAction("ui_accept"))
            {
                this.GUIManager?.BringToFront(this.Name);
            }
        }

        protected void GrabPlayer()
        {
            if (this.Player is null)
            {
                this.Player = GlobalConstants.GameManager?.Player;

                if (this.Player is null)
                {
                    return;
                }

                this.Player.HappinessChange -= this.SetHappiness;
                this.Player.HappinessChange += this.SetHappiness;

                GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;
                GlobalConstants.GameManager.SettingsManager.ValueChanged += this.SettingChanged;
                
                this.EnableHappiness = (bool) GlobalConstants.GameManager.SettingsManager
                    .Get(SettingsManager.HAPPINESS_UI)
                    .ObjectValue;
                
                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = this.Player.OverallHappiness
                });
            }
        }

        protected void SettingChanged(object sender, ValueChangedEventArgs<object> args)
        {
            if (args.Name.Equals(SettingsManager.HAPPINESS_UI))
            {
                this.EnableHappiness = (bool) args.NewValue;
                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = this.Player.OverallHappiness
                });
            }
        }

        protected void SetHappiness(object sender, ValueChangedEventArgs<float> args)
        {
            float happiness = this.EnableHappiness ? args.NewValue : 1f;

            try
            {
                if (this.Material is ShaderMaterial shaderMaterial)
                {
                    shaderMaterial.SetShaderParam("happiness", happiness);
                }

                foreach (CanvasItem child in this.GetAllChildren())
                {
                    var material = child.Material as ShaderMaterial;
                    material?.SetShaderParam("happiness", happiness);
                }
            }
            catch (Exception e)
            {
                GD.PushError("Object has been disposed!");
            }
        }

        public virtual void Display()
        {
            this.GrabPlayer();
            this.Show();
            Array children = this.GetAllChildren();
            foreach (var child in children)
            {
                if (child is Node node)
                {
                    node.SetProcess(true);
                    node.SetProcessInput(true);
                    node.SetPhysicsProcess(true);
                }
            }

            this.OnGUIOpen?.Invoke(this);
        }

        public virtual bool Close(object sender)
        {
            if (this.AlwaysOpen)
            {
                return false;
            }

            this.Hide();
            Array children = this.GetAllChildren();
            foreach (var child in children)
            {
                if (child is Node node)
                {
                    node.SetProcess(false);
                    node.SetProcessInput(false);
                    node.SetPhysicsProcess(false);
                }
            }

            this.OnGUIClose?.Invoke(this);
            return true;
        }

        public override void _Ready()
        {
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            
            this.GrabPlayer();
        }

        public virtual void ButtonClose()
        {
            this.GUIManager.CloseGUI(this, this.Name);
        }

        public override void _ExitTree()
        {
            if (this.Player is null == false)
            {
                this.Player.HappinessChange -= this.SetHappiness;
            }

            GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;
            
            base._ExitTree();
        }
    }
}