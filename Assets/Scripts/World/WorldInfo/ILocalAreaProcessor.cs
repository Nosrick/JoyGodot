using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public interface ILocalAreaProcessor : ITagged
    {
        string Get(IWorldInstance worldInstance, IEntity origin = null);
    }
}