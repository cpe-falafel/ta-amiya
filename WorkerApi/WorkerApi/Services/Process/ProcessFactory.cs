using System.Diagnostics;

namespace WorkerApi.Services.Process
{
    public class ProcessFactory : IProcessFactory
    {
        public IProcessWrapper CreateProcess() => new ProcessWrapper();

        public IEnumerable<IProcessWrapper> GetProcessesByName(string processName)
        {
            return System.Diagnostics.Process.GetProcessesByName(processName)
                .Select(p => new ProcessWrapper());
        }
    }
}
