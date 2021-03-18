using System;
using System.Collections.Generic;
using JoyLib.Code.Entities.Items;

namespace JoyLib.Code.Entities.Abilities
{
    public class FoodItem : AbstractAbility
    {
        public FoodItem() : base(
            "Eat",
            "fooditem",
            "Sate your hunger with a tasty meal.",
            false,
            1,
            1,
            1,
            false,
            new string[] { "fulfillneedaction" },
            new Tuple<string, int>[0],
            GetPrerequisites(),
            AbilityTarget.Self,
            0,
            GetSprite("fooditem"),
            "active", "ingestion")
        { }

        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("food", 1);
            return prereqs;
        }
        
        public override bool OnUse(IEntity user, IJoyObject target)
        {
            if(target is IItemInstance item)
            {
                this.m_CachedActions["fulfillneedaction"].Execute(
                    new IJoyObject[] { user },
                    new string[] { "hunger", "need", "fulfill" },
                    new Dictionary<string, object>
                    {
                        {"need", "hunger"}, 
                        {"value" , item.ItemType.Value}, 
                        {"counter", 10}
                    }
                );
                user.RemoveContents(item);
                GlobalConstants.GameManager.ItemHandler?.Destroy(item.Guid);

                if (user.PlayerControlled)
                {
                    GlobalConstants.GameManager.GUIManager?.CloseAllGUIs();
                }
                return true;
            }
            return false;
        }
    }
}