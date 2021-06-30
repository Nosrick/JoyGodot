using System;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.World;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.IO
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
                string json = JSON.Print(world.Save(), "\t");
                File.WriteAllText(directory + "/world.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.QuestTracker.Save(), "\t");
                File.WriteAllText(directory + "/quests.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.ItemHandler.Save(), "\t");
                File.WriteAllText(directory + "/items.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.EntityHandler.Save(), "\t");
                File.WriteAllText(directory + "/entities.dat", json);

                json = JSON.Print(GlobalConstants.GameManager.RelationshipHandler.Save(), "\t");
                File.WriteAllText(directory + "/relationships.dat", json);
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

            string json = File.ReadAllText(directory + "/relationships.dat");
            Dictionary tempDict = this.GetDictionary(json);
            GlobalConstants.GameManager.RelationshipHandler.Load(tempDict);

            json = File.ReadAllText(directory + "/entities.dat");
            tempDict = this.GetDictionary(json);
            GlobalConstants.GameManager.EntityHandler.Load(tempDict);

            json = File.ReadAllText(directory + "/items.dat");
            tempDict = this.GetDictionary(json);
            GlobalConstants.GameManager.ItemHandler.Load(tempDict);

            json = File.ReadAllText(directory + "/quests.dat");
            tempDict = this.GetDictionary(json);
            GlobalConstants.GameManager.QuestTracker.Load(tempDict);
            
            json = File.ReadAllText(directory + "/world.dat");
            tempDict = this.GetDictionary(json);
            IWorldInstance overworld = new WorldInstance();
            overworld.Load(tempDict);

            return overworld;
        }

        protected Dictionary GetDictionary(string data)
        {
            JSONParseResult result = JSON.Parse(data);

            if (result.Error != Error.Ok)
            {
                GD.PushError("Could not parse JSON!\n" +
                             "At line: " + result.ErrorLine + "\n" +
                             "Error message: " + result.ErrorString);
                return null;
            }

            if (result.Result is Dictionary dictionary)
            {
                return dictionary;
            }
            
            GD.PushError("Could not parse JSON into dictionary!");
            return null;
        }
    }
}
