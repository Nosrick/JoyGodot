using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public interface IItemMaterial : ITagged
    {
        string Name { get; }
        
        float Hardness { get; }
        
        int Bonus { get; }
        
        float Density { get; }
        
        float ValueMod { get; }
    }
}