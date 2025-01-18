namespace WorkerApi.Services.Process
{
    public interface IProcessFactory
    {
        IProcessWrapper CreateProcess();
        IEnumerable<IProcessWrapper> GetProcessesByName(string processName);
    }
}
