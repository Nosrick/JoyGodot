using System;
using Code.Collections;
using Godot;
using JoyLib.Code.Combat;
using JoyLib.Code.Conversation;
using JoyLib.Code.Conversation.Subengines.Rumours;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Managers;
using JoyLib.Code.Physics;
using JoyLib.Code.Quests;
using JoyLib.Code.Rollers;
using JoyLib.Code.States;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;
using JoyLib.Code.World;

namespace JoyLib.Code
{
    public interface IGameManager : IDisposable
    {
        bool Initialised { get; }
        int LoadingPercentage { get; }
        
        string LoadingMessage { get; }
        
        //CheatInterface Cheats { get; set; }
        
        ActionLog ActionLog { get; }
        ICombatEngine CombatEngine { get; }
        IQuestTracker QuestTracker { get; }
        IQuestProvider QuestProvider { get; }
        IEntityRelationshipHandler RelationshipHandler { get; }
        IEntityStatisticHandler StatisticHandler { get; }
        IMaterialHandler MaterialHandler { get; }
        IObjectIconHandler ObjectIconHandler { get; }
        ICultureHandler CultureHandler { get; }
        IEntityTemplateHandler EntityTemplateHandler { get; }
        IEntityBioSexHandler BioSexHandler { get; }
        IEntitySexualityHandler SexualityHandler { get; }
        IGenderHandler GenderHandler { get; }
        IJobHandler JobHandler { get; }
        IEntityRomanceHandler RomanceHandler { get; }
        IGUIManager GUIManager { get; }
        IParameterProcessorHandler ParameterProcessorHandler { get; }
        ILiveEntityHandler EntityHandler { get; }
        ILiveItemHandler ItemHandler { get; }
        IItemDatabase ItemDatabase { get; }
        INeedHandler NeedHandler { get; }
        IEntitySkillHandler SkillHandler { get; }
        IWorldInfoHandler WorldInfoHandler { get; }
        IPhysicsManager PhysicsManager { get; }
        IConversationEngine ConversationEngine { get; }
        IAbilityHandler AbilityHandler { get; }
        IDerivedValueHandler DerivedValueHandler { get; }
        IVisionProviderHandler VisionProviderHandler { get; }
        
        GUIDManager GUIDManager { get; }
        
        IRumourMill RumourMill { get; }
        
        NaturalWeaponHelper NaturalWeaponHelper { get; }
        
        RNG Roller { get; }
        
        IEntityFactory EntityFactory { get; }
        IItemFactory ItemFactory { get; }
        
        Node MyNode { get; }
        
        IEntity Player { get; }
        GameObjectPool<ManagedSprite> FloorPool { get; }
        GameObjectPool<JoyObjectNode> WallPool { get; }
        GameObjectPool<JoyObjectNode> EntityPool { get; }
        GameObjectPool<JoyObjectNode> ItemPool { get; }
        GameObjectPool<Sprite> FogPool { get; }
        void SetNextState(IGameState nextState = null);

        void Reset();
    }
}