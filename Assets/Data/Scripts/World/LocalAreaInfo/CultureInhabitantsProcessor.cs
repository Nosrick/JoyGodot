using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.World;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Data.Scripts.World.LocalAreaInfo
{
    public class CultureInhabitantsProcessor : AbstractLocalAreaProcessor
    {
        public override string Get(IWorldInstance worldInstance, IEntity origin = null)
        {
            var entityGrouping = worldInstance.Entities
                .SelectMany(entity => entity.Cultures)
                .Distinct()
                .Select(culture => new Tuple<ICulture, int>(
                    culture,
                    worldInstance.Entities.Count(entity => entity.Cultures.Contains(culture))));

            List<string> stringData = new List<string>();
            
            foreach (var group in entityGrouping)
            {
                int count = group.Item2;
                if (count > 10)
                {
                    int remainder = count % 10;
                    count -= remainder;
                }

                bool plural = count > 1;

                string temp = "There " + 
                              (plural ? "are " : "is ") +
                              (plural ? "roughly " : "") + 
                              count +
                              (plural ? " people " : " person ") + 
                              "who " +
                              (plural ? "belong" : "belongs") +
                              " to the " + group.Item1.CultureName.ToTitleCase() + " here.";
                stringData.Add(temp);
            }

            return string.Join(" ", stringData);
        }
    }
}