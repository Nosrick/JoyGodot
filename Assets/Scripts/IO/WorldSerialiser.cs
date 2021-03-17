using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Collections;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Managers;
using JoyLib.Code.Quests;
using JoyLib.Code.World;
using Sirenix.OdinSerializer;

namespace JoyLib.Code.IO
{
    public class WorldSerialiser
    {
        protected IObjectIconHandler ObjectIcons { get; set; }

        protected const DataFormat DEFAULT_DATA_FORMAT = DataFormat.Binary;

        public WorldSerialiser(IObjectIconHandler objectIcons)
        {
            this.ObjectIcons = objectIcons;
        }
        
        public void Serialise(IWorldInstance world)
        {
            string directory = Directory.GetCurrentDirectory() + "/save/" + world.Name;
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.AddText("Cannot open save directory.", LogLevel.Error);
                GlobalConstants.ActionLog.StackTrace(e);
            }
            try
            {
                byte[] array = SerializationUtility.SerializeValue(world, DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/world.dat", array);
                /*
                StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/save/" + world.Name + "/sav.dat", false);
                JsonSerializer serializer = JsonSerializer.CreateDefault();
                serializer.Serialize(writer, world);
                writer.Close();
                */

                array = SerializationUtility.SerializeValue(GlobalConstants.GameManager.QuestTracker.AllQuests,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/quests.dat", array);

                array = SerializationUtility.SerializeValue(GlobalConstants.GameManager.ItemHandler.QuestRewards,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/rewards.dat", array);

                array = SerializationUtility.SerializeValue(
                    GlobalConstants.GameManager.RelationshipHandler.Values,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/relationships.dat", array);

                array = SerializationUtility.SerializeValue(
                    GlobalConstants.GameManager.ItemHandler.Values,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/items.dat", array);

                array = SerializationUtility.SerializeValue(
                    GlobalConstants.GameManager.EntityHandler.Values,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/entities.dat", array);

                array = SerializationUtility.SerializeValue(
                    GlobalConstants.GameManager.GUIDManager,
                    DEFAULT_DATA_FORMAT);
                File.WriteAllBytes(directory + "/guids.dat", array);
                
                File.WriteAllText(directory + "/data_format.dat", DEFAULT_DATA_FORMAT.ToString());
            }
            catch(Exception e)
            {
                GlobalConstants.ActionLog.AddText("Cannot serialise and/or write world to file.", LogLevel.Error);
                GlobalConstants.ActionLog.StackTrace(e);
            }
        }

        public IWorldInstance Deserialise(string worldName)
        {
            /*
            StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "/save/" + worldName + "/sav.dat");
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            IWorldInstance world = serializer.Deserialize<IWorldInstance>(new JsonTextReader(reader));
            reader.Close();
            */
            

            string directory = Directory.GetCurrentDirectory() + "/save/" + worldName;

            if (!Enum.TryParse(File.ReadAllText(directory + "/data_format.dat"), out DataFormat dataFormat))
            {
                dataFormat = DEFAULT_DATA_FORMAT;
            }
            
            byte[] array = File.ReadAllBytes(directory + "/world.dat");
            IWorldInstance world = SerializationUtility.DeserializeValue<IWorldInstance>(array, dataFormat);
            world.Initialise();

            array = File.ReadAllBytes(directory + "/items.dat");
            IEnumerable<IItemInstance> items =
                SerializationUtility.DeserializeValue<IEnumerable<IItemInstance>>(array, dataFormat);
            this.Items(items, world);

            array = File.ReadAllBytes(directory + "/entities.dat");
            IEnumerable<IEntity> entities =
                SerializationUtility.DeserializeValue<IEnumerable<IEntity>>(array, dataFormat);
            this.Entities(entities, world);

            array = File.ReadAllBytes(directory + "/relationships.dat");
            IEnumerable<IRelationship> relationships =
                SerializationUtility.DeserializeValue<IEnumerable<IRelationship>>(array, dataFormat);
            this.Relationships(relationships);

            array = File.ReadAllBytes(directory + "/quests.dat");
            IEnumerable<IQuest> quests = SerializationUtility.DeserializeValue<IEnumerable<IQuest>>(array, dataFormat);
            this.Quests(quests);

            array = File.ReadAllBytes(directory + "/rewards.dat");
            NonUniqueDictionary<Guid, Guid> rewards =
                SerializationUtility.DeserializeValue<NonUniqueDictionary<Guid, Guid>>(array, dataFormat);
            this.QuestRewards(rewards);

            array = File.ReadAllBytes(directory + "/guids.dat");
            GUIDManager guidManager =
                SerializationUtility.DeserializeValue<GUIDManager>(array, dataFormat);

            GlobalConstants.GameManager.GUIDManager.Deserialise(guidManager.RecycleList);
            
            this.LinkWorlds(world);
            this.AssignIcons(world);
            
            return world;
        }

        private void LinkWorlds(IWorldInstance parent)
        {
            foreach (IWorldInstance world in parent.Areas.Values)
            {
                world.Parent = parent;
                this.LinkWorlds(world);
            }
        }
        
        private void AssignIcons(IWorldInstance parent)
        {
            foreach (IJoyObject wall in parent.Walls.Values)
            {
                foreach (ISpriteState state in wall.States)
                {
                    this.SetUpSpriteStates(wall.TileSet, state);
                }

                wall.MyWorld = parent;
            }

            foreach(IWorldInstance world in parent.Areas.Values)
            {
                this.AssignIcons(world);
            }
        }

        protected void SetUpSpriteStates(string tileSet, ISpriteState state)
        {
            foreach (SpritePart part in state.SpriteData.m_Parts)
            {
                part.m_FrameSprites = this.ObjectIcons.GetRawFrames(tileSet, state.Name, part.m_Name, state.SpriteData.m_State);
            }
        }

        protected void HandleContents(IItemContainer container)
        {
            foreach (IItemInstance item in container.Contents)
            {
                foreach (ISpriteState state in item.States)
                {
                    this.SetUpSpriteStates(item.TileSet, state);
                }

                foreach (IItemInstance content in item.Contents)
                {
                    foreach (ISpriteState state in content.States)
                    {
                        this.SetUpSpriteStates(content.TileSet, state);
                    }
                    this.HandleContents(content);
                }
            }
        }

        private void QuestRewards(NonUniqueDictionary<Guid, Guid> rewards)
        {
            foreach (Guid questID in rewards.Keys)
            {
                GlobalConstants.GameManager.ItemHandler.AddQuestRewards(questID, rewards.FetchValuesForKey(questID));
            }
        }

        private void Quests(IEnumerable<IQuest> quests)
        {
            foreach (IQuest quest in quests)
            {
                GlobalConstants.GameManager.QuestTracker.AddQuest(quest.Questor, quest);
            }
        }

        private void Relationships(IEnumerable<IRelationship> relationships)
        {
            foreach (IRelationship relationship in relationships)
            {
                GlobalConstants.GameManager.RelationshipHandler.Add(relationship);
            }
        }

        private void Items(IEnumerable<IItemInstance> items, IWorldInstance overworld)
        {
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld);
            foreach (IItemInstance item in items)
            {
                item.Deserialise();
                
                foreach (ISpriteState state in item.States)
                {
                    this.SetUpSpriteStates(item.TileSet, state);
                }

                item.MyWorld = worlds.FirstOrDefault(world => world.ItemGUIDs.Contains(item.Guid));
                
                GlobalConstants.GameManager.ItemHandler.Add(item);
            }
        }

        private void Entities(IEnumerable<IEntity> entities, IWorldInstance overworld)
        {
            List<IWorldInstance> worlds = overworld.GetWorlds(overworld);
            foreach (IEntity entity in entities)
            {
                List<ICulture> cultures = entity.CultureNames.Select(name => GlobalConstants.GameManager.CultureHandler.GetByCultureName(name)).ToList();

                entity.Deserialise(cultures);
                
                foreach (ISpriteState state in entity.States)
                {
                    this.SetUpSpriteStates(entity.TileSet, state);
                }

                foreach (INeed need in entity.Needs.Values)
                {
                    need.FulfillingSprite = new SpriteState(
                        need.Name, 
                        GlobalConstants.GameManager.ObjectIconHandler.GetFrame(
                            "needs", 
                            need.Name));
                }

                worlds.First(world => world.EntityGUIDs.Contains(entity.Guid))?.AddEntity(entity);
            }
        }
    }
}
