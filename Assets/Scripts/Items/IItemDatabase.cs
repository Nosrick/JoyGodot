﻿using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Items
{
    public interface IItemDatabase : IHandler<BaseItemType, string>
    {
        IDictionary<string, int> ItemWeights { get; }
        
        IEnumerable<BaseItemType> FindItemsOfType(string[] tags, int tolerance = 1);

        IEnumerable<BaseItemType> GetAllForName(string name);
    }
}