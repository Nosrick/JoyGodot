using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public class LocalAreaInfoHandler : ILocalAreaInfoHandler
    {
        protected List<ILocalAreaProcessor> Processors { get; set; }

        public LocalAreaInfoHandler()
        {
            this.Processors = GlobalConstants.ScriptingEngine
                .FetchAndInitialiseChildren<ILocalAreaProcessor>()
                .ToList();
        }
        
        public string GetRandomLocalAreaInfo(IWorldInstance world)
        {
            return this.Processors.GetRandom().Get(world);
        }

        public string GetSpecificLocalAreaInfo(IWorldInstance world, IEnumerable<string> tags)
        {
            ILocalAreaProcessor processor = this.Processors.Where(
                p => p.HasTags(tags))
                .ToArray()
                .GetRandom();

            return processor.Get(world);
        }
        
        public void Dispose()
        {
            this.Processors = null;
        }
    }
}