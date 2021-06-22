using Godot;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class MoveContainerPriority : Resource
    {
        [Export] public string m_ContainerName;
        [Export] public int m_Priority;
        [Export] public bool m_RequiresVisibility;

        public MoveContainerPriority()
        {
        }
    }
}