using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.Entities
{
    public class LiveEntityHandler : ILiveEntityHandler
    {
        protected IDictionary<Guid, IEntity> m_Entities;
        protected IEntity m_Player;

        public IEnumerable<IEntity> Values => this.m_Entities.Values.ToList();
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public LiveEntityHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.m_Entities = new Dictionary<Guid, IEntity>();
        }

        public bool Add(IEntity created)
        {
            try
            {
                if (this.m_Entities.ContainsKey(created.Guid))
                {
                    return false;
                }
                
                this.m_Entities.Add(created.Guid, created);

                if(created.PlayerControlled)
                {
                    this.m_Player = created;
                }

                return true;
            }
            catch(Exception e)
            {
                GlobalConstants.ActionLog.StackTrace(e);
                return false;
            }
        }

        public bool Destroy(Guid key)
        {
            if (!this.m_Entities.ContainsKey(key))
            {
                return false;
            }
            //this.m_Entities[key].Dispose();
            this.m_Entities[key] = null;
            this.m_Entities.Remove(key);
            return true;

        }

        public IEntity Get(Guid GUID)
        {
            return this.m_Entities.TryGetValue(GUID, out IEntity entity) ? entity : null;
        }

        public IEnumerable<IEntity> Load()
        {
            return new List<IEntity>();
        }

        public IEntity GetPlayer()
        {
            return this.m_Player;
        }

        public void SetPlayer(IEntity entity)
        {
            this.m_Player = entity;
        }

        public void ClearLiveEntities()
        {
            this.m_Entities = new Dictionary<Guid, IEntity>();
        }

        public void Dispose()
        {
            GarbageMan.Dispose(this.m_Entities);
            this.m_Entities = null;
        }
    }
}
