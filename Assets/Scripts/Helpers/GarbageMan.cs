﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class GarbageMan
    {
        public static void Dispose<T, K>(IDictionary<T, K> collection)
        {
            if (collection is null)
            {
                return;
            }
            
            var keys = collection.Keys.ToArray();
            foreach (var key in keys)
            {
                collection[key] = default;
            }
        }
    }
}