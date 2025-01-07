namespace WorkerApi.Services
{
    public interface ICommandBuildService
    {
        string BuildCommand(string jsonWorkerConfiguration);
    }
}