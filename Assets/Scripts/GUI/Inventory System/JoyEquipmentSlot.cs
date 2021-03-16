using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyEquipmentSlot : JoyItemSlot
    {
        [Export] protected Label m_SlotName;

        public Label SlotName => this.m_SlotName;
    }
}