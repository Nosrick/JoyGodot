namespace JoyGodot.Assets.Scripts.Entities.Needs
{
    public interface IObjectProvider<T> where T : INeed
    {
        JoyObject.JoyObject[] FindFulfillmentObject(Entity searcher);
    }
}
