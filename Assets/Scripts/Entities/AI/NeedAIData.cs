using System;

using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.AI
{
    public struct NeedAIData : ISerialisationHandler
    {
        public IJoyObject Target { get; set; }
        public Vector2Int TargetPoint { get; set; }
        public bool Searching { get; set; }
        public Intent Intent { get; set; }
        public bool Idle { get; set; }
        public string Need { get; set; }

        public static NeedAIData IdleState()
        {
            return new NeedAIData
            {
                Target = null,
                TargetPoint = GlobalConstants.NO_TARGET,
                Searching = false,
                Idle = true,
                Intent = Intent.Interact,
                Need = "none"
            };
        }

        public static NeedAIData SearchingState()
        {
            return new NeedAIData
            {
                Target = null,
                TargetPoint = GlobalConstants.NO_TARGET,
                Searching = true,
                Idle = false,
                Intent = Intent.Interact,
                Need = "none"
            };
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"TargetPoint", this.TargetPoint.Save()},
                {"Target", this.Target?.Guid.ToString()},
                {"Searching", this.Searching},
                {"Intent", this.Intent.ToString()},
                {"Idle", this.Idle},
                {"Need", this.Need}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;
            this.TargetPoint = new Vector2Int(valueExtractor.GetValueFromDictionary<Dictionary>(data, "TargetPoint"));
            string target = valueExtractor.GetValueFromDictionary<string>(data, "Target");
            Guid guid = target.IsNullOrEmpty() ? Guid.Empty : new Guid(target);

            IJoyObject tempTarget = GlobalConstants.GameManager.EntityHandler.Get(guid) 
                                    ?? (IJoyObject) GlobalConstants.GameManager.ItemHandler.Get(guid);

            this.Target = tempTarget;

            this.Searching = valueExtractor.GetValueFromDictionary<bool>(data, "Searching");
            this.Intent = (Intent) Enum.Parse(
                typeof(Intent),
                valueExtractor.GetValueFromDictionary<string>(
                    data, 
                    "Intent"));

            this.Idle = valueExtractor.GetValueFromDictionary<bool>(data, "Idle");
            this.Need = valueExtractor.GetValueFromDictionary<string>(data, "Need");
        }
    }
}