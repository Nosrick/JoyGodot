using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using JoyLib.Code.Helpers;
using JoyLib.Code.Scripting;
using Directory = System.IO.Directory;

namespace JoyLib.Code.Entities.AI.LOS.Providers
{
    public class VisionProviderHandler : IVisionProviderHandler
    {
        protected Dictionary<string, IVision> VisionTypes { get; set; }
        
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
                Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Vision Types",
                "*.json",
                SearchOption.AllDirectories);

            /*
            foreach (string file in files)
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken["VisionTypes"])
                            {
                                string name = (string) child["Name"];
                                Color lightColour = child["LightColour"] is null
                                    ? Color.white
                                    : GraphicsHelper.ParseHTMLString((string) child["LightColour"]);

                                Color darkColour = child["DarkColour"] is null
                                    ? Color.black
                                    : GraphicsHelper.ParseHTMLString((string) child["DarkColour"]);

                                string fovHandler = (string) child["Handler"] ?? "FOVShadowCasting";
                                IFOVHandler handler =
                                    (IFOVHandler) ScriptingEngine.Instance.FetchAndInitialise(fovHandler);

                                int minimumLight = (int) (child["MinimumLight"] ?? 0);
                                int maximumLight = (int) (child["MaximumLight"] ?? GlobalConstants.MAX_LIGHT);

                                int minimumComfort = (int) (child["MinimumComfort"] ?? minimumLight);
                                int maximumComfort = (int) (child["MaximumComfort"] ?? maximumLight);

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
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not parse vision type in file ", LogLevel.Error);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                    }
                }
            }
*/

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
            this.VisionTypes = null;
        }
    }
}