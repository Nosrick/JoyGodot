using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Collections
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
            lock (this.InactiveObjects)
            {
                if (this.InactiveObjects.Count > 0)
                {
                    T returnObject = this.InactiveObjects.First();
                    this.InactiveObjects.RemoveAt(0);
                    this.Objects.Add(returnObject);
                    returnObject.SetProcess(true);

                    foreach (Node child in returnObject.GetAllChildren())
                    {
                        child.SetProcess(true);
                            
                        if (child is CanvasItem canvasItem)
                        {
                            canvasItem.Show();
                        }
                    }
                    
                    return returnObject;
                }
            }

            T newObject = this.Prefab.Instance<T>();
            this.Objects.Add(newObject);
            this.Parent.AddChild(newObject);
            return newObject;
        }

        public bool Retire(T gameObject)
        {
            lock (this.Objects)
            {
                bool result = this.Objects.Remove(gameObject);
                if (result)
                {
                    lock (this.InactiveObjects)
                    {
                        GD.Print("Retiring " + gameObject.Name + " at " + this.Parent.Name);
                        gameObject.Name = "InactiveObject" + this.InactiveObjects.Count;
                        gameObject.SetProcess(false);
                        gameObject.Hide();

                        foreach (Node child in gameObject.GetAllChildren())
                        {
                            child.SetProcess(false);
                            
                            if (child is CanvasItem canvasItem)
                            {
                                canvasItem.Hide();
                            }
                        }
                        
                        this.InactiveObjects.Add(gameObject);

                        if (gameObject.Visible)
                        {
                            GD.PushWarning("Retired node is still visible! " + gameObject.Name);
                        }
                    }
                }
                else
                {
                    GD.PushWarning("Could not remove object " + gameObject?.Name + " from " + this.Parent.Name);
                }

                return result;
            }
        }

        public void RetireAll()
        {
            var clone = new List<T>(this.Objects);
            foreach (T gameObject in clone)
            {
                this.Retire(gameObject);
            }

            if (this.InactiveObjects.Any(obj => obj.Visible))
            {
                GD.PushWarning("For some reason, not every node is invisible after being retired. " + this.Parent.Name);
                var list = this.InactiveObjects.Where(obj => obj.Visible).ToList();
                GD.PushWarning("Offenders:\n" + GlobalConstants.ActionLog.CollectionWalk(list));
            }
        }
    }
}