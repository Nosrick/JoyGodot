using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Items
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