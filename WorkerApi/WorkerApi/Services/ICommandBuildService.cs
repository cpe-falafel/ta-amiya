using WorkerApi.Models;

namespace WorkerApi.Services
{
    public interface ICommandBuildService
    {
        VideoCommand BuildCommand(string jsonWorkerConfiguration);
    }
}