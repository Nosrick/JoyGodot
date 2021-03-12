using System;
using System.Collections.Generic;
using JoyLib.Code.Collections;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities.Items
{
    public interface ILiveItemHandler : IHandler<IItemInstance, Guid>
    {
        bool Add(IItemInstance item);
        bool AddItems(IEnumerable<IItemInstance> item, bool addToWorld = false);

        bool RemoveItemFromWorld(Guid GUID);

        bool RemoveItemFromWorld(IItemInstance item);

        bool AddItemToWorld(WorldInstance world, Guid GUID);
        IEnumerable<IItemInstance> GetQuestRewards(Guid questID);

        void CleanUpRewards();
        void AddQuestReward(Guid questID, Guid reward);
        void AddQuestRewards(Guid questID, IEnumerable<Guid> rewards);
        void AddQuestRewards(Guid questID, IEnumerable<IItemInstance> rewards);

        void ClearLiveItems();

        IEnumerable<IItemInstance> GetItems(IEnumerable<Guid> guids);
        
        IEnumerable<IItemInstance> Values { get; }
        
        NonUniqueDictionary<Guid, Guid> QuestRewards { get; }
    }
}