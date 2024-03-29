﻿using System;
using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class TradeAction : AbstractAction
    {
        public override bool Execute(
            IJoyObject[] participants, 
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            if (participants.Length != 2)
            {
                return false;
            }

            if (args.IsNullOrEmpty())
            {
                return false;
            }

            if (!(participants[0] is IEntity)
                && !(participants[1] is IEntity))
            {
                return false;
            }
            
            IEntity left = participants[0] as IEntity;
            IEntity right = participants[1] as IEntity;

            IEnumerable<IItemInstance> leftOffering = new IItemInstance[0];
            if (args.TryGetValue("leftOffering", out object arg))
            {
                leftOffering = (IEnumerable<IItemInstance>) arg;
            }
            IEnumerable<IItemInstance> rightOffering = new IItemInstance[0];
            if (args.TryGetValue("rightOffering", out arg))
            {
                rightOffering = (IEnumerable<IItemInstance>) arg;
            }

            if (left is null == false)
            {
                left.AddContents(rightOffering);
                left.RemoveContents(leftOffering);
            }

            if (right is null == false)
            {
                right.AddContents(leftOffering);
                right.RemoveContents(rightOffering);
            }

            HashSet<string> myTags = tags is null ? new HashSet<string>() : new HashSet<string>(tags);
            if (myTags.Any(tag => tag.Equals("trade", StringComparison.OrdinalIgnoreCase)) == false)
            {
                myTags.Add("trade");
            }

            if (myTags.Any(tag => tag.Equals("give", StringComparison.OrdinalIgnoreCase)) == false)
            {
                myTags.Add("give");
            }
            
            if (myTags.Any(tag => tag.Equals("item", StringComparison.OrdinalIgnoreCase)) == false)
            {
                myTags.Add("item");
            }

            int rightValue = rightOffering.Sum(instance => instance.Value);
            int leftValue = leftOffering.Sum(instance => instance.Value);
            if (leftValue < rightValue - 50)
            {
                myTags.Add("negative");
            }
            else if (leftValue > rightValue + 50)
            {
                myTags.Add("positive");
            }
            
            this.SetLastParameters(participants, myTags, args);
            return true;
        }
    }
}