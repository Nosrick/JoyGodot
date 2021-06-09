using System;
using System.Collections.Generic;
using Godot;
using JoyLib.Code.Godot;
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
        IGuidHolder,
        ITickable,
        ISerialisationHandler
    {
        List<ISpriteState> States { get; }
        bool IsDestructible { get; }
        bool IsWall { get; }
        string JoyName { get; }
        
        string TileSet { get; }
        
        IRollable Roller { get; }
        
        IWorldInstance MyWorld { get; set; }
        
        List<IJoyAction> CachedActions { get; }
        
        JoyObjectNode MyNode { get; set; }
        
        ICollection<string> Tooltip { get; }
        
        void Update();

        IJoyAction FetchAction(string name);

        void SetStates(IEnumerable<ISpriteState> states);
    }
}