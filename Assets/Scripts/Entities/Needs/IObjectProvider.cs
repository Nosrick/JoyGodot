namespace JoyLib.Code.Entities.Needs
{
    public interface IObjectProvider<T> where T : INeed
    {
        JoyObject[] FindFulfillmentObject(Entity searcher);
    }
}
