using System;
using System.Collections;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities.AI
{
    public class Pathfinder
    {
        public Queue<Vector2Int> FindPath(Vector2Int from, Vector2Int to, IWorldInstance worldRef)
        {
            IWorldInstance world = worldRef;

            HashSet<Vector2Int> walls = world.Walls;

            AStar pathfinder = new AStar();

            AStarNode.sizes = new Vector2Int(world.Tiles.GetLength(0), world.Tiles.GetLength(1));
            AStarNode.walls = walls;
            AStarNode goalNode = new AStarNode2D(null, null, 0, to.x, to.y);
            AStarNode startNode = new AStarNode2D(null, goalNode, 0, from.x, from.y);
            pathfinder.FindPath(startNode, goalNode);

            Queue<Vector2Int> path = new Queue<Vector2Int>();

            for (int i = 0; i < pathfinder.solution.Count; i++)
            {
                AStarNode2D node = (AStarNode2D)pathfinder.solution[i];
                path.Enqueue(new Vector2Int(node.x, node.y));
            }

            return path;
        }

        public static Sector DetermineSector(Vector2Int from, Vector2Int to)
        {
            float xDiff = to.x - from.x;
            float yDiff = to.y - from.y;
            double angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            angle += 90;

            if (angle < 0)
            {
                angle += 360;
            }

            if((angle >= 0 && angle <= 22.5) || (angle <= 360 && angle >= 337.5))
            {
                return Sector.North;
            }
            else if(angle <= 67.5)
            {
                return Sector.NorthEast;
            }
            else if(angle <= 112.5)
            {
                return Sector.East;
            }
            else if(angle <= 157.5)
            {
                return Sector.SouthEast;
            }
            else if(angle <= 202.5)
            {
                return Sector.South;
            }
            else if(angle <= 247.5)
            {
                return Sector.SouthWest;
            }
            else if(angle <= 292.5)
            {
                return Sector.West;
            }
            else if(angle <= 337.5)
            {
                return Sector.NorthWest;
            }

            return Sector.Centre;
        }
    }

    public class AStarNode : IComparable
    {
        #region Properties

        public static Vector2Int sizes
        {
            get;
            set;
        }

        public static HashSet<Vector2Int> walls
        {
            protected get;
            set;
        }

        private AStarNode m_Parent;
        /// <summary>
        /// The parent of the node.
        /// </summary>
        public AStarNode parent
        {
            get
            {
                return this.m_Parent;
            }
            set
            {
                this.m_Parent = value;
            }
        }

        /// <summary>
        /// The accumulative cost of the path until now.
        /// </summary>
        public double cost
        {
            set
            {
                this.m_Cost = value;
            }
            get
            {
                return this.m_Cost;
            }
        }
        private double m_Cost;

        /// <summary>
        /// The estimated cost to the goal from here.
        /// </summary>
        public double goalEstimate
        {
            set
            {
                this.m_GoalEstimate = value;
            }
            get
            {
                this.Calculate();
                return (this.m_GoalEstimate);
            }
        }
        private double m_GoalEstimate;

        /// <summary>
        /// The cost plus the estimated cost to the goal from here.
        /// </summary>
        public double totalCost
        {
            get
            {
                return (this.cost + this.goalEstimate);
            }
        }

        /// <summary>
        /// The goal node.
        /// </summary>
        public AStarNode goalNode
        {
            set
            {
                this.m_GoalNode = value;
                this.Calculate();
            }
            get
            {
                return this.m_GoalNode;
            }
        }
        private AStarNode m_GoalNode;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentRef">The node's parent</param>
        /// <param name="goalRef">The goal node</param>
        /// <param name="costRef">The accumulative cost until now</param>
        public AStarNode(AStarNode parentRef, AStarNode goalRef, double costRef)
        {
            this.m_Parent = parentRef;
            this.m_Cost = costRef;
            this.goalNode = goalRef;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Determines wheather the current node is the goal.
        /// </summary>
        /// <returns>Returns true if current node is the goal</returns>
        public bool IsGoal()
        {
            return this.IsSameState(this.m_GoalNode);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="node">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public virtual bool IsSameState(AStarNode node)
        {
            return false;
        }

        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public virtual void Calculate()
        {
            this.m_GoalEstimate = 0.0f;
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="successorsRef">List in which the successors will be added</param>
        public virtual void GetSuccessors(ArrayList successorsRef)
        {
        }

        /// <summary>
        /// Prints information about the current node
        /// </summary>
        public virtual void PrintNodeInfo()
        {
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            return this.IsSameState((AStarNode)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return (-this.totalCost.CompareTo(((AStarNode)obj).totalCost));
        }

        #endregion
    }

    /// <summary>
    /// Class for performing A* pathfinding
    /// </summary>
    public sealed class AStar
    {
        #region Private Fields

        private AStarNode m_StartNode;
        private AStarNode m_GoalNode;
        private Heap m_OpenList;
        private Heap m_ClosedList;
        private ArrayList m_Successors;

        private const int LOOPBREAKER = 100;

        #endregion

        #region Properties

        /// <summary>
        /// Holds the solution after pathfinding is done. <see>FindPath()</see>
        /// </summary>
        public ArrayList solution
        {
            get
            {
                return this.m_Solution;
            }
        }
        private ArrayList m_Solution;

        #endregion

        #region Constructors

        public AStar()
        {
            this.m_OpenList = new Heap();
            this.m_ClosedList = new Heap();
            this.m_Successors = new ArrayList();
            this.m_Solution = new ArrayList();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds the shortest path from the start node to the goal node
        /// </summary>
        /// <param name="startNode">Start node</param>
        /// <param name="goalNode">Goal node</param>
        public void FindPath(AStarNode startNode, AStarNode goalNode)
        {
            this.m_StartNode = startNode;
            this.m_GoalNode = goalNode;

            int loopBreak = 0;

            this.m_OpenList.Add(this.m_StartNode);
            while (this.m_OpenList.Count > 0 && loopBreak < LOOPBREAKER)
            {
                // Get the node with the lowest TotalCost
                AStarNode NodeCurrent = (AStarNode) this.m_OpenList.Pop();

                // If the node is the goal copy the path to the solution array
                if (NodeCurrent.IsGoal())
                {
                    while (NodeCurrent != null)
                    {
                        this.m_Solution.Insert(0, NodeCurrent);
                        NodeCurrent = NodeCurrent.parent;
                    }
                    break;
                }

                // Get successors to the current node
                NodeCurrent.GetSuccessors(this.m_Successors);
                foreach (AStarNode NodeSuccessor in this.m_Successors)
                {
                    // Test if the currect successor node is on the open list, if it is and
                    // the TotalCost is higher, we will throw away the current successor.
                    AStarNode NodeOpen = null;
                    if (this.m_OpenList.Contains(NodeSuccessor))
                    {
                        NodeOpen = (AStarNode) this.m_OpenList[this.m_OpenList.IndexOf(NodeSuccessor)];
                    }
                    if ((NodeOpen != null) && (NodeSuccessor.totalCost > NodeOpen.totalCost))
                    {
                        continue;
                    }

                    // Test if the currect successor node is on the closed list, if it is and
                    // the TotalCost is higher, we will throw away the current successor.
                    AStarNode NodeClosed = null;
                    if (this.m_ClosedList.Contains(NodeSuccessor))
                    {
                        NodeClosed = (AStarNode) this.m_ClosedList[this.m_ClosedList.IndexOf(NodeSuccessor)];
                    }
                    if ((NodeClosed != null) && (NodeSuccessor.totalCost > NodeClosed.totalCost))
                    {
                        continue;
                    }

                    // Remove the old successor from the open list
                    this.m_OpenList.Remove(NodeOpen);

                    // Remove the old successor from the closed list
                    this.m_ClosedList.Remove(NodeClosed);

                    // Add the current successor to the open list
                    this.m_OpenList.Push(NodeSuccessor);
                }
                // Add the current node to the closed list
                this.m_ClosedList.Add(NodeCurrent);
                loopBreak += 1;
            }
        }

        #endregion
    }

    public class AStarNode2D : AStarNode
    {
        #region Properties

        /// <summary>
        /// The x-coordinate of the node
        /// </summary>
        public int x
        {
            get
            {
                return this.m_X;
            }
        }
        private int m_X;

        /// <summary>
        /// The y-coordinate of the node
        /// </summary>
        public int y
        {
            get
            {
                return this.m_Y;
            }
        }
        private int m_Y;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for a node in a 2-dimensional map
        /// </summary>
        /// <param name="parentRef">Parent of the node</param>
        /// <param name="goalRef">Goal node</param>
        /// <param name="costRef">Accumulative cost</param>
        /// <param name="AX">x-coordinate</param>
        /// <param name="AY">y-coordinate</param>
        public AStarNode2D(AStarNode parentRef, AStarNode goalRef, double costRef, int AX, int AY) : 
            base(parentRef, goalRef, costRef)
        {
            this.m_X = AX;
            this.m_Y = AY;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a successor to a list if it is not impassible or the parent node
        /// </summary>
        /// <param name="successorsRef">List of successors</param>
        /// <param name="AX">x-coordinate</param>
        /// <param name="AY">y-coordinate</param>
        private void AddSuccessor(ArrayList successorsRef, int AX, int AY)
        {
            int CurrentCost = 1;

            if (AX < 0 || AX >= sizes.x)
                return;

            if (AY < 0 || AY >= sizes.y)
                return;

            Vector2Int point = new Vector2Int(AX, AY);
            if (walls.Contains(point))
            {
                return;
            }

            if (CurrentCost == -1)
            {
                return;
            }
            AStarNode2D NewNode = new AStarNode2D(this, this.goalNode, this.cost + CurrentCost, AX, AY);
            if (NewNode.IsSameState(this.parent))
            {
                return;
            }
            successorsRef.Add(NewNode);
        }

        #endregion

        #region Overidden Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="node">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public override bool IsSameState(AStarNode node)
        {
            if (node == null)
            {
                return false;
            }
            return ((((AStarNode2D)node).x == this.m_X) && (((AStarNode2D)node).y == this.m_Y));
        }

        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public override void Calculate()
        {
            if (this.goalNode != null)
            {
                double xd = this.m_X - ((AStarNode2D) this.goalNode).x;
                double yd = this.m_Y - ((AStarNode2D) this.goalNode).y;
                // "Euclidean distance" - Used when search can move at any angle.
                //GoalEstimate = Math.Sqrt((xd*xd) + (yd*yd));
                // "Manhattan Distance" - Used when search can only move vertically and 
                // horizontally.
                //GoalEstimate = Math.Abs(xd) + Math.Abs(yd); 
                // "Diagonal Distance" - Used when the search can move in 8 directions.
                this.goalEstimate = Math.Max(Math.Abs(xd), Math.Abs(yd));
            }
            else
            {
                this.goalEstimate = 0;
            }
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="ASuccessors">List in which the successors will be added</param>
        public override void GetSuccessors(ArrayList ASuccessors)
        {
            ASuccessors.Clear();
            this.AddSuccessor(ASuccessors, this.m_X - 1, this.m_Y);
            this.AddSuccessor(ASuccessors, this.m_X - 1, this.m_Y - 1);
            this.AddSuccessor(ASuccessors, this.m_X, this.m_Y - 1);
            this.AddSuccessor(ASuccessors, this.m_X + 1, this.m_Y - 1);
            this.AddSuccessor(ASuccessors, this.m_X + 1, this.m_Y);
            this.AddSuccessor(ASuccessors, this.m_X + 1, this.m_Y + 1);
            this.AddSuccessor(ASuccessors, this.m_X, this.m_Y + 1);
            this.AddSuccessor(ASuccessors, this.m_X - 1, this.m_Y + 1);
        }

        #endregion
    }
}