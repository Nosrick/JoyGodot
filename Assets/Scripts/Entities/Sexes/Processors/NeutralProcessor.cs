﻿using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Entities.Sexes.Processors
{
    public class NeutralProcessor : IBioSexProcessor
    {
        public string Name => "neutral";
        
        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }
    }
}