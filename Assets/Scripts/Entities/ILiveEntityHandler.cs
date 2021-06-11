using System;
using System.Collections.Generic;

namespace JoyLib.Code.Entities
{
    public interface ILiveEntityHandler : IHandler<IEntity, Guid>, ISerialisationHandler
    {
        IEntity GetPlayer();

        void SetPlayer(IEntity entity);

        void ClearLiveEntities();
    }
}