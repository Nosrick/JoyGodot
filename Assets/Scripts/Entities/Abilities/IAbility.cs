using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.Entities.Abilities
{
    public interface IAbility : ITagged, ISerialisationHandler
    {
        //When the ability is added to the user
        bool OnAdd(IEntity entity);

        //When the ability is removed from the user
        bool OnRemove(IEntity entity);

        //When the entity attacks
        bool OnAttack(
            IEntity attacker, 
            IEntity target, 
            IEnumerable<string> attackerTags, 
            IEnumerable<string> defenderTags);

        //When the entity is hit
        int OnTakeHit(
            IEntity attacker, 
            IEntity defender, 
            int damage, 
            IEnumerable<string> attackerTags, 
            IEnumerable<string> defenderTags);

        //When the entity heals
        int OnHeal(
            IEntity receiver, 
            IEntity healer, 
            int healing, 
            IEnumerable<string> receiverTags, 
            IEnumerable<string> healerTags);

        //When the entity picks up an item
        bool OnPickup(IEntity entity, IItemInstance item);

        //When the entity ticks
        bool OnTick(IEntity entity);

        //When the entity reduces another entity to zero of a Derived Value
        bool OnReduceToZero(IEntity attacker, IEntity target, IDerivedValue value);

        //When the entity reduces another entity to the "disabled" status of a Derived Value
        bool OnDisable(IEntity attacker, IEntity target, IDerivedValue value);

        //When the entity uses an item
        bool OnUse(IEntity user, IJoyObject target);

        //When the entity interacts with something
        bool OnInteract(IEntity actor, IJoyObject observer);

        //When the entity uses a skill
        //This returns the success threshold modification for the roll
        //The second parameter is for checking against other possible stat/skill values
        int OnCheckRollModifyThreshold(
            int successThreshold, 
            IEnumerable<IBasicValue<int>> values, 
            IEnumerable<string> attackerTags, 
            IEnumerable<string> defenderTags);

        //This returns bonus/penalty dice for the roll
        //The second parameter is for checking against other possible stat/skill values
        int OnCheckRollModifyDice(
            int dicePool, 
            IEnumerable<IBasicValue<int>> values, 
            IEnumerable<string> attackerTags, 
            IEnumerable<string> defenderTags);

        //This is used for directly modifying the successes of the check
        //And should return the new successes
        //The second parameter is for checking against other possible stat/skill values
        int OnCheckSuccess(
            int successes, 
            IEnumerable<IBasicValue<int>> values, 
            IEnumerable<string> attackerTags, 
            IEnumerable<string> defenderTags);

        bool DecrementCounter(int value);

        bool DecrementMagnitude(int value);

        void IncrementMagnitude(int value);

        void IncrementCounter(int value);

        bool EnactToll(IEntity caster);

        bool MeetsPrerequisites(IEntity actor);

        bool MeetsPrerequisites(IEnumerable<IBasicValue<int>> data);

        bool IsInRange(IEntity left, IJoyObject right);

        string Name
        {
            get;
        }

        string InternalName
        {
            get;
        }

        string Description
        {
            get;
        }

        bool Stacking
        {
            get;
        }

        int Counter
        {
            get;
        }

        int Magnitude
        {
            get;
        }

        int Priority
        {
            get;
        }

        bool ReadyForRemoval
        {
            get;
        }

        bool Permanent
        {
            get;
        }

        IEnumerable<Tuple<string, int>> Costs
        {
            get;
        }

        IDictionary<string, int> Prerequisites
        {
            get;
        }

        AbilityTarget TargetType
        {
            get;
        }
        
        int Range { get; }
        
        SpriteData SpriteData { get; }
        
        Texture UsingIcon { get; }
    }
}
