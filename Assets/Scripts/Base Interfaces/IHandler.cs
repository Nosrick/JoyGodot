using System;
using System.Collections.Generic;
using JoyLib.Code.Helpers;

namespace JoyLib.Code
{
    public interface IHandler<TData, TKey> : IDisposable
    {
        IEnumerable<TData> Values { get; }
        
        JSONValueExtractor ValueExtractor { get; }

        TData Get(TKey name);
        bool Add(TData value);
        bool Destroy(TKey key);
        IEnumerable<TData> Load();
    }
}