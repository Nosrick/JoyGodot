using System;
using System.Collections.Generic;
using System.Linq;

namespace JoyLib.Code.Helpers
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
                if (collection[key] is IDisposable disposeOfMe)
                {
                    disposeOfMe.Dispose();
                }
                collection[key] = default;
            }
        }
    }
}