using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Items
{
    public interface ILiveItemHandler : IHandler<IItemInstance, Guid>, ISerialisationHandler
    {
        bool AddItems(IEnumerable<IItemInstance> item, bool addToWorld = false);

        bool RemoveItemFromWorld(Guid GUID);

        bool RemoveItemFromWorld(IItemInstance item);

        bool AddItemToWorld(WorldInstance world, Guid GUID);
        IEnumerable<IItemInstance> GetQuestRewards(Guid questID);

        void CleanUpRewards();
        void AddQuestRewards(Guid questID, IEnumerable<IItemInstance> rewards);

        void ClearLiveItems();

        IEnumerable<IItemInstance> GetItems(IEnumerable<Guid> guids);

        NonUniqueDictionary<Guid, Guid> QuestRewards { get; }
    }
}