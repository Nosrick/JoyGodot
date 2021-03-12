using System;
using System.Collections.Generic;

namespace JoyLib.Code.Entities
{
    public interface ILiveEntityHandler : IHandler<IEntity, Guid>
    {
        IEntity GetPlayer();

        void SetPlayer(IEntity entity);

        void ClearLiveEntities();
    }
}