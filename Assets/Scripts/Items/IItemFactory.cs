namespace JoyGodot.Assets.Scripts.Items
{
    public interface IItemFactory
    {
        IItemInstance CreateRandomItemOfType(string[] tags = null, bool identified = false);
        IItemInstance CreateSpecificType(string name, string[] tags, bool identified = false);
        IItemInstance CreateRandomWeightedItem(bool identified = false, bool withAbility = false);
        IItemInstance CreateFromTemplate(BaseItemType itemType, bool identified = false);
    }
}