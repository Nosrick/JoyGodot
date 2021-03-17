using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Conversation.Subengines.Rumours.Parameters
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
            return new List<IParameterProcessor>(ScriptingEngine.Instance.FetchAndInitialiseChildren<IParameterProcessor>());
        }

        public IParameterProcessor Get(string parameter)
        {
            return this.Parameters.First(p => p.CanParse(parameter));
        }
    }
}