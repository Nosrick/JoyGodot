using System;
using System.Collections.Generic;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Godot;

namespace JoyLib.Code.Entities.Items
{
    public interface IItemInstance : IJoyObject, IItemContainer, IOwnable
    {
        IEnumerable<IAbility> UniqueAbilities { get; }
        
        IEnumerable<IAbility> AllAbilities { get; }

        IItemInstance Copy(IItemInstance copy);

        void Interact(IEntity user, string ability);

        void IdentifyMe();

        void Deserialise();

        void Instantiate(bool recursive = true, JoyObjectNode gameObject = null, bool active = false);
        
        bool Identified { get; }
        
        bool Broken { get; }
        
        int Efficiency { get; }
        
        string ConditionString { get; }
        
        string SlotString { get; }
        
        string IdentifiedName { get; }
        
        string DisplayDescription { get; }
        
        float Weight { get; }
        
        string WeightString { get; }
        
        string DisplayName { get; }
        
        BaseItemType ItemType { get; }
        
        int Value { get; }

        bool InWorld { get; set; }
    }
}