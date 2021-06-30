namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public interface IItemMaterial
    {
        string Name { get; }
        
        float Hardness { get; }
        
        int Bonus { get; }
        
        float Density { get; }
        
        float ValueMod { get; }
    }
}