using System;
using System.Collections.Generic;

namespace JoyLib.Code
{
    public interface IHandler<T, K> : IDisposable
    {
        IEnumerable<T> Values { get; }

        T Get(K name);
        bool Add(T value);
        bool Destroy(K key);
        IEnumerable<T> Load();
    }
}