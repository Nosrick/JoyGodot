using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Jobs
{
    public interface IJobHandler : IHandler<IJob, string>
    {
        IJob GetRandom();
    }
}