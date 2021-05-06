using System.Collections.Generic;
using Godot;

namespace Code.Collections
{
    public class GameObjectPool<T> where T : Node2D
    {
        /// <summary>
        /// The active GameObjects
        /// </summary>
        protected List<T> Objects { get; set; }
        
        /// <summary>
        /// The GameObjects which have been 'retired'
        /// </summary>
        protected List<T> InactiveObjects { get; set; }
        
        /// <summary>
        /// The prefab we use to instantiate
        /// </summary>
        protected PackedScene Prefab { get; set; }
        
        /// <summary>
        /// This is the GameObject that new instances will be parented to
        /// </summary>
        protected Node2D Parent { get; set; }

        public GameObjectPool(PackedScene prefab, Node2D parent)
        {
            this.Objects = new List<T>();
            this.InactiveObjects = new List<T>();
            this.Prefab = prefab;
            this.Parent = parent;
        }

        public GameObjectPool(T prefab, Node2D parent)
        {
            this.Objects = new List<T>();
            this.InactiveObjects = new List<T>();
            this.Prefab = new PackedScene();
            this.Prefab.Pack(prefab);
            this.Parent = parent;
        }

        public T Get()
        {
            if (this.InactiveObjects.Count > 0)
            {
                T returnObject = this.InactiveObjects[0];
                this.InactiveObjects.RemoveAt(0);
                this.Objects.Add(returnObject);
                returnObject.SetProcess(true);
                return returnObject;
            }

            T newObject = this.Prefab.Instance<T>();
            this.Objects.Add(newObject);
            this.Parent.AddChild(newObject);
            return newObject;
        }

        public bool Retire(T gameObject)
        {
            bool result = this.Objects.Remove(gameObject);
            if (result)
            {
                gameObject.SetProcess(false);
                this.InactiveObjects.Add(gameObject);
                gameObject.Visible = false;
                this.Parent.RemoveChild(gameObject);
            }
            

            return result;
        }

        public void RetireAll()
        {
            foreach (T gameObject in this.Objects)
            {
                gameObject.Visible = false;
            }
            this.InactiveObjects.AddRange(this.Objects);
            this.Objects.Clear();
        }
    }
}