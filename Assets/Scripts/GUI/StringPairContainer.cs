using System;
using Castle.Core.Internal;
using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public class StringPairContainer : Node2D
    {
        [Export] protected Label m_Key;
        [Export] protected Label m_Value;
        
        protected Tuple<string, string> m_Target;

        public void OnEnable()
        {
        }

        public Tuple<string, string> Target
        {
            get => this.m_Target;
            set
            {
                this.m_Target = value;
                this.Repaint();
            }
        }

        protected virtual void Repaint()
        {
            this.m_Key.Text = this.Target.Item1;
            this.m_Value.Text = this.Target.Item2;

            this.m_Key.Visible = !this.m_Key.Text.IsNullOrEmpty();
            this.m_Value.Visible = !this.m_Value.Text.IsNullOrEmpty();

            this.m_Value.Align = this.Target.Item1.IsNullOrEmpty()
                ? Label.AlignEnum.Center
                : Label.AlignEnum.Left;

            this.m_Key.Align = this.Target.Item2.IsNullOrEmpty()
                ? Label.AlignEnum.Center
                : Label.AlignEnum.Right;
        }
    }
}