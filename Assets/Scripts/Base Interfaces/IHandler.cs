using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface IHandler<TData, TKey> : IDisposable
    {
        IEnumerable<TData> Values { get; }
        
        JSONValueExtractor ValueExtractor { get; }

        TData Get(TKey name);
        IEnumerable<TData> GetMany(IEnumerable<TKey> keys);
        bool Add(TData value);
        bool Destroy(TKey key);
        IEnumerable<TData> Load();
    }
}