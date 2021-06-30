using System;
using Godot;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Combat;
using JoyGodot.Assets.Scripts.Conversation;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.Quests;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Settings;
using JoyGodot.Assets.Scripts.States;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts
{
    public interface IGameManager : IDisposable
    {
        bool Initialised { get; }
        int LoadingPercentage { get; }
        
        string LoadingMessage { get; }
        
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
        IWorldHandler WorldHandler { get; }
        
        SettingsManager SettingsManager { get; }
        
        GUIDManager GUIDManager { get; }
        
        IRumourMill RumourMill { get; }
        
        NaturalWeaponHelper NaturalWeaponHelper { get; }
        
        RNG Roller { get; }
        
        IEntityFactory EntityFactory { get; }
        IItemFactory ItemFactory { get; }
        
        Node MyNode { get; }
        
        IEntity Player { get; }
        TileMap FloorTileMap { get; }
        TileMap WallTileMap { get; }
        GameObjectPool<JoyObjectNode> EntityPool { get; }
        GameObjectPool<JoyObjectNode> ItemPool { get; }
        GameObjectPool<PositionableSprite> FogPool { get; }
        Node2D FogHolder { get; }
        
        Node2D WorldHolder { get; }
        Node2D EntityHolder { get; }
        Node2D ItemHolder { get; }
        
        void SetNextState(IGameState nextState = null);

        void RetireAll();

        void Reset();
    }
}