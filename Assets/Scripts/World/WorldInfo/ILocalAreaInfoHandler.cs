using System;
using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public interface ILocalAreaInfoHandler : IDisposable
    {
        string GetRandomLocalAreaInfo(IWorldInstance world);

        string GetSpecificLocalAreaInfo(IWorldInstance world, IEnumerable<string> tags);
    }
}