using System.Collections.Generic;

namespace JoyLib.Code.Entities.Items
{
    public interface IItemDatabase : IHandler<BaseItemType, string>
    {
        IDictionary<string, int> ItemWeights { get; }
        
        IEnumerable<BaseItemType> FindItemsOfType(string[] tags, int tolerance = 1);
    }
}