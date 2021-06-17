using System;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities
{
    public interface ILiveEntityHandler : IHandler<IEntity, Guid>, ISerialisationHandler
    {
        IEntity GetPlayer();

        void SetPlayer(IEntity entity);

        void ClearLiveEntities();
    }
}