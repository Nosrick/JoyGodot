using System;
using Castle.Core.Internal;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.AI
{
    public class NeedAIData : ISerialisationHandler
    {
        public IJoyObject target;
        public Vector2Int targetPoint;
        public bool searching;
        public Intent intent;
        public bool idle;
        public string need;

        public static NeedAIData IdleState()
        {
            return new NeedAIData
            {
                target = null,
                targetPoint = GlobalConstants.NO_TARGET,
                searching = false,
                idle = true,
                intent = Intent.Interact,
                need = "none"
            };
        }

        public static NeedAIData SearchingState()
        {
            return new NeedAIData
            {
                target = null,
                targetPoint = GlobalConstants.NO_TARGET,
                searching = true,
                idle = false,
                intent = Intent.Interact,
                need = "none"
            };
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"TargetPoint", this.targetPoint.Save()},
                {"Target", this.target?.Guid.ToString()},
                {"Searching", this.searching},
                {"Intent", this.intent.ToString()},
                {"Idle", this.idle},
                {"Need", this.need}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;
            this.targetPoint = new Vector2Int(valueExtractor.GetValueFromDictionary<Dictionary>(data, "TargetPoint"));
            string target = valueExtractor.GetValueFromDictionary<string>(data, "Target");
            Guid guid = target.IsNullOrEmpty() ? Guid.Empty : new Guid(target);

            IJoyObject tempTarget = GlobalConstants.GameManager.EntityHandler.Get(guid) 
                                    ?? (IJoyObject) GlobalConstants.GameManager.ItemHandler.Get(guid);

            this.target = tempTarget;

            this.searching = valueExtractor.GetValueFromDictionary<bool>(data, "Searching");
            this.intent = (Intent) Enum.Parse(
                typeof(Intent),
                valueExtractor.GetValueFromDictionary<string>(
                    data, 
                    "Intent"));

            this.idle = valueExtractor.GetValueFromDictionary<bool>(data, "Idle");
            this.need = valueExtractor.GetValueFromDictionary<string>(data, "Need");
        }
    }
}