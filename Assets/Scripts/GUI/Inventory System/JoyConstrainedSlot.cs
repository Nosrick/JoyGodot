using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public class JoyConstrainedSlot : JoyItemSlot
    {
        [Export] public string Slot { get; set; }
        
        public Label SlotLabel { get; protected set; }
    }
}