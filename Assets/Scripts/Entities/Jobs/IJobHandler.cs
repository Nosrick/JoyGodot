using System.Collections.Generic;

namespace JoyLib.Code.Entities.Jobs
{
    public interface IJobHandler : IHandler<IJob, string>
    {
        IJob GetRandom();
    }
}