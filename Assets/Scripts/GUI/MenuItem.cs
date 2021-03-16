using Code.Unity.GUI.Managed_Assets;
using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public class MenuItem : ManagedButton
    {
        [Export] protected Label m_Text;

        public Label Text
        {
            get => this.m_Text;
            protected set => this.m_Text = value;
        }
    }
}