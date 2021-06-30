﻿using System.Collections.Generic;

namespace JoyGodot.Assets.Scripts.Entities.Sexes.Processors
{
    public class FemaleProcessor : IBioSexProcessor
    {
        public string Name => "female";

        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }
    }
}