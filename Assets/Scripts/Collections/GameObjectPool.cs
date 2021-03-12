using System.Collections.Generic;
using Godot;

namespace Code.Collections
{
    public class GameObjectPool
    {
        /// <summary>
        /// The active GameObjects
        /// </summary>
        protected List<Node2D> Objects { get; set; }
        
        /// <summary>
        /// The GameObjects which have been 'retired'
        /// </summary>
        protected List<Node2D> InactiveObjects { get; set; }
        
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
            this.Objects = new List<Node2D>();
            this.InactiveObjects = new List<Node2D>();
            this.Prefab = prefab;
            this.Parent = parent;
        }

        public Node2D Get()
        {
            if (this.InactiveObjects.Count > 0)
            {
                Node2D returnObject = this.InactiveObjects[0];
                this.InactiveObjects.RemoveAt(0);
                this.Objects.Add(returnObject);
                return returnObject;
            }

            Node2D newObject = (Node2D) this.Prefab.Instance();
            this.Objects.Add(newObject);
            return newObject;
        }

        public bool Retire(Node2D gameObject)
        {
            bool result = this.Objects.Remove(gameObject);
            if (result)
            {
                this.InactiveObjects.Add(gameObject);
                gameObject.Visible = false;
            }

            return result;
        }

        public void RetireAll()
        {
            foreach (Node2D gameObject in this.Objects)
            {
                gameObject.Visible = false;
            }
            this.InactiveObjects.AddRange(this.Objects);
            this.Objects.Clear();
        }
    }
}