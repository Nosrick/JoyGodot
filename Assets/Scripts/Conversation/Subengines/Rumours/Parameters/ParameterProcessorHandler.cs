using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters
{
    public class ParameterProcessorHandler : IParameterProcessorHandler
    {
        protected List<IParameterProcessor> Parameters { get; set; }

        public ParameterProcessorHandler()
        {
            this.Parameters = this.LoadProcessors();
        }

        protected List<IParameterProcessor> LoadProcessors()
        {
            return new List<IParameterProcessor>(GlobalConstants.ScriptingEngine.FetchAndInitialiseChildren<IParameterProcessor>());
        }

        public IParameterProcessor Get(string parameter)
        {
            return this.Parameters.First(p => p.CanParse(parameter));
        }
    }
}