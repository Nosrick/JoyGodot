using System;
using System.Collections.Generic;
using Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;

namespace JoyLib.Code
{
    public interface IJoyObject : 
        ITagged, 
        IPosition, 
        IDerivedValueContainer, 
        IDataContainer, 
        IDisposable, 
        IGuidHolder,
        ITickable
    {
        List<ISpriteState> States { get; }
        bool IsDestructible { get; }
        bool IsWall { get; }
        string JoyName { get; }
        
        string TileSet { get; }
        
        IRollable Roller { get; }
        
        IWorldInstance MyWorld { get; set; }
        
        List<IJoyAction> CachedActions { get; }
        
        Node2D MyNode { get; }
        
        IEnumerable<Tuple<string, string>> Tooltip { get; }
        
        void Update();

        IJoyAction FetchAction(string name);

        void SetStates(IEnumerable<ISpriteState> states);
    }
}