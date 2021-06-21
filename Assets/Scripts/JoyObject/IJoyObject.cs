using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.JoyObject
{
    public interface IJoyObject : 
        ITagged, 
        IPosition, 
        IDerivedValueContainer, 
        IDataContainer, 
        IGuidHolder,
        ITickable,
        ISerialisationHandler,
        IJoyNameHolder
    {
        List<ISpriteState> States { get; }
        bool IsDestructible { get; }
        bool IsWall { get; }
        
        string TileSet { get; }
        
        Guid WorldGuid { get; set; }
        
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