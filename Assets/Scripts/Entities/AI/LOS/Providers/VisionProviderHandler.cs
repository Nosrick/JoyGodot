using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers
{
    public class VisionProviderHandler : IVisionProviderHandler
    {
        protected System.Collections.Generic.Dictionary<string, IVision> VisionTypes { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IEnumerable<IVision> Values => this.VisionTypes.Values;

        public VisionProviderHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.VisionTypes = this.Load().ToDictionary(vision => vision.Name, vision => vision);
        }

        public IEnumerable<IVision> Load()
        {
            List<IVision> visionTypes = new List<IVision>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER + 
                GlobalConstants.DATA_FOLDER + 
                "Vision Types",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));
                
                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary visionDict))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> visions = this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                    this.ValueExtractor.GetValueFromDictionary<Array>(
                        visionDict, 
                        "VisionTypes"));

                foreach (Dictionary innerDict in visions)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                    Color lightColour =
                        new Color(this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "LightColour"));
                    Color darkColour =
                        new Color(this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "DarkColour"));
                    int minimumLight = innerDict.Contains("MinimumLight")
                    ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "MinimumLight")
                    : 0;
                    int minimumComfort = innerDict.Contains("MinimumComfort") 
                    ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "MinimumComfort")
                    : minimumLight;
                    int maximumLight = innerDict.Contains("MaximumLight") 
                    ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "MaximumLight")
                    : GlobalConstants.MAX_LIGHT;
                    int maximumComfort = innerDict.Contains("MaximumComfort")
                    ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "MaximumComfort")
                    : maximumLight;

                    string visionProvider = innerDict.Contains("Algorithm")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Algorithm")
                        : nameof(FOVShadowCasting);

                    IFOVHandler handler = (IFOVHandler) GlobalConstants.ScriptingEngine.FetchAndInitialise(visionProvider);
                    
                    visionTypes.Add(
                        new BaseVisionProvider(
                            darkColour,
                            lightColour,
                            handler,
                            minimumLight,
                            minimumComfort,
                            maximumLight,
                            maximumComfort,
                            name));
                }
            }

            return visionTypes;
        }

        public bool AddVision(
            string name, 
            Color darkColour, 
            Color lightColour, 
            IFOVHandler algorithm, 
            int minimumLightLevel = 0,
            int minimumComfortLevel = 0,
            int maximumLightLevel = GlobalConstants.MAX_LIGHT,
            int maximumComfortLevel = GlobalConstants.MAX_LIGHT)
        {
            if (this.VisionTypes.ContainsKey(name))
            {
                return false;
            }

            this.VisionTypes.Add(
                name,
                new BaseVisionProvider(
                    darkColour,
                    lightColour,
                    algorithm,
                    minimumLightLevel,
                    minimumComfortLevel,
                    maximumLightLevel,
                    maximumComfortLevel,
                    name));

            return true;
        }

        public bool AddVision(IVision vision)
        {
            if (this.VisionTypes.ContainsKey(vision.Name))
            {
                return false;
            }

            this.VisionTypes.Add(vision.Name, vision);
            return true;
        }

        public bool HasVision(string name)
        {
            return this.VisionTypes.ContainsKey(name);
        }

        public IVision Get(string name)
        {
            if (this.VisionTypes.ContainsKey(name))
            {
                return this.VisionTypes[name].Copy();
            }

            throw new InvalidOperationException("Could not find vision type with name " + name);
        }

        public IEnumerable<IVision> GetMany(IEnumerable<string> keys)
        {
            return keys.Select(this.Get);
        }

        public bool Add(IVision value)
        {
            if (this.VisionTypes.ContainsKey(value.Name))
            {
                return false;
            }

            this.VisionTypes.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (!this.VisionTypes.ContainsKey(key))
            {
                return false;
            }
            
            this.VisionTypes[key] = null;
            this.VisionTypes.Remove(key);
            return true;
        }

        public void Dispose()
        {
            GarbageMan.Dispose(this.VisionTypes);
            this.VisionTypes = null;
        }
    }
}