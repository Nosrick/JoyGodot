using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code.Collections;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Quests;
using JoyLib.Code.World;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.IO
{
    public class WorldSerialiser
    {
        protected IObjectIconHandler ObjectIcons { get; set; }

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
                GlobalConstants.ActionLog.Log("Cannot open save directory.", LogLevel.Error);
                GlobalConstants.ActionLog.StackTrace(e);
            }
            try
            {
                string json = JSON.Print(world, "\t");
                File.WriteAllText(directory + "/world.dat", json);
                /*
                StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "/save/" + world.Name + "/sav.dat", false);
                JsonSerializer serializer = JsonSerializer.CreateDefault();
                serializer.Serialize(writer, world);
                writer.Close();
                */

                json = JSON.Print(GlobalConstants.GameManager.QuestTracker.AllQuests, "\t");
                File.WriteAllText(directory + "/quests.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.ItemHandler.QuestRewards, "\t");
                File.WriteAllText(directory + "/rewards.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.RelationshipHandler.Values, "\t");
                File.WriteAllText(directory + "/relationships.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.ItemHandler.Values, "\t");
                File.WriteAllText(directory + "/items.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.EntityHandler.Values, "\t");
                File.WriteAllText(directory + "/entities.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.GUIDManager, "\t");
                File.WriteAllText(directory + "/guids.dat", json);
            }
            catch(Exception e)
            {
                GlobalConstants.ActionLog.Log("Cannot serialise and/or write world to file.", LogLevel.Error);
                GlobalConstants.ActionLog.StackTrace(e);
            }
        }

        public IWorldInstance Deserialise(string worldName)
        {
            string directory = Directory.GetCurrentDirectory() + "/save/" + worldName;

            string json = File.ReadAllText(directory + "/world.dat");
            /*
            IWorldInstance world = JSON.Parse<IWorldInstance>(json);
            world.Initialise();

            json = File.ReadAllText(directory + "/items.dat");
            IEnumerable<IItemInstance> items =
                JSON.Parse<IEnumerable<IItemInstance>>(json);
            this.Items(items, world);

            json = File.ReadAllText(directory + "/entities.dat");
            IEnumerable<IEntity> entities =
                JSON.Parse<IEnumerable<IEntity>>(json);
            this.Entities(entities, world);

            json = File.ReadAllText(directory + "/relationships.dat");
            IEnumerable<IRelationship> relationships =
                JSON.Parse<IEnumerable<IRelationship>>(json);
            this.Relationships(relationships);

            json = File.ReadAllText(directory + "/quests.dat");
            IEnumerable<IQuest> quests = JSON.Parse<IEnumerable<IQuest>>(json);
            this.Quests(quests);

            json = File.ReadAllText(directory + "/rewards.dat");
            NonUniqueDictionary<Guid, Guid> rewards =
                JSON.Parse<NonUniqueDictionary<Guid, Guid>>(json);
            this.QuestRewards(rewards);

            json = File.ReadAllText(directory + "/guids.dat");
            GUIDManager guidManager =
                JSON.Parse<GUIDManager>(json);

            GlobalConstants.GameManager.GUIDManager.Deserialise(guidManager.RecycleList);
            this.LinkWorlds(world);
            this.AssignIcons(world);
            
            return world;
            */
            return null;
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
            /*
            foreach (IJoyObject wall in parent.Walls.Values)
            {
                foreach (ISpriteState state in wall.States)
                {
                    this.SetUpSpriteStates(wall.TileSet, state);
                }

                wall.MyWorld = parent;
            }
            */

            foreach(IWorldInstance world in parent.Areas.Values)
            {
                this.AssignIcons(world);
            }
        }

        protected void SetUpSpriteStates(string tileSet, ISpriteState state)
        {
            foreach (SpritePart part in state.SpriteData.Parts)
            {
                //part.m_FrameSprite = this.ObjectIcons.(tileSet, state.Name, part.m_Name, state.SpriteData.m_State);
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
                    /*
                    need.FulfillingSprite = new SpriteState(
                        need.Name, 
                        GlobalConstants.GameManager.ObjectIconHandler.GetFrame(
                            "needs", 
                            need.Name));
                            */
                }

                worlds.First(world => world.EntityGUIDs.Contains(entity.Guid))?.AddEntity(entity);
            }
        }
    }
}
