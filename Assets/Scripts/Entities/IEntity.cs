using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI;
using JoyGodot.Assets.Scripts.Entities.AI.Drivers;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Scripts.Entities
{
    public interface IEntity : IJoyObject, IItemContainer
    {
        IDictionary<string, IEntityStatistic> Statistics { get; }
        IDictionary<string, IEntitySkill> Skills { get; }
        IDictionary<string, INeed> Needs { get; }
        List<IAbility> Abilities { get; }
        List<IAbility> AllAbilities { get; }
        EquipmentStorage Equipment { get; }
        IItemInstance NaturalWeapons { get; }
        IBioSex Sex { get; }
        ISexuality Sexuality { get; }
        IRomance Romance { get; }
        
        IGender Gender { get; }
        
        List<string> IdentifiedItems { get; }

        IJob CurrentJob { get; }
        List<IJob> Jobs { get; }
        List<string> Slots { get; }
        List<ICulture> Cultures { get; }
        int Size { get; }
        
        IVision VisionProvider { get; }
        NeedFulfillmentData NeedFulfillmentData { get; set; }
        NeedAIData CurrentTarget { get; set;  }
        IDriver Driver { get; }
        IPathfinder Pathfinder { get; }
        Queue<Vector2Int> PathfindingData { get; set; }
        
        Vector2Int TargetPoint { get; set; }
        IAbility TargetingAbility { get; set; }
        
        bool PlayerControlled { get; set; }
        bool Sentient { get; }
        
        int VisionMod { get; }
        
        IEnumerable<Vector2Int> Vision { get; }
        
        string CreatureType { get; }
        
        bool HasMoved { get; set; }

        bool Conscious { get; }
        
        bool Alive { get; }
        
        List<string> CultureNames { get; }
        
        float OverallHappiness { get; }
        
        bool HappinessIsDirty { get; set; }

        void Deserialise(
            IEnumerable<ICulture> cultures);
        
        void AddQuest(IQuest quest);
        IEnumerable<Tuple<string, object>> GetData(IEnumerable<string> tags, params object[] args);
        void AddIdentifiedItem(string nameRef);
        bool RemoveItemFromPerson(IItemInstance item);
        bool RemoveEquipment(IItemInstance item);
        IItemInstance[] SearchBackpackForItemType(IEnumerable<string> tags);
        bool EquipItem(IItemInstance itemRef);
        IItemInstance GetEquipment(string slotRef);
        bool UnequipItem(IItemInstance actor);

        void AddExperience(int value);

        bool AddJob(IJob job);
        bool ChangeJob(string job);
        bool ChangeJob(IJob job);
        
        void DamageMe(int value, Entity source);
        event ValueChangedEventHandler<int> StatisticChange;
        event ValueChangedEventHandler<int> SkillChange;
        event ValueChangedEventHandler<int> ExperienceChange;
        event JobChangedEventHandler JobChange;
        event BooleanChangedEventHandler ConsciousnessChange;

        event BooleanChangedEventHandler AliveChange;

        event ValueChangedEventHandler<float> HappinessChange;

        event ValueChangedEventHandler<int> NeedChange;
    }
}