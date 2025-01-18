using System.Diagnostics;

namespace WorkerApi.Services.Process
{
    public interface IProcessWrapper : IDisposable
    {
        bool HasExited { get; }
        ProcessStartInfo StartInfo { get; set; }
        bool EnableRaisingEvents { get; set; }
        int ProcessId { get; }
        void Start();
        void Kill();
        void BeginOutputReadLine();
        void BeginErrorReadLine();
        Task WaitForExitAsync();
        event DataReceivedEventHandler OutputDataReceived;
        event DataReceivedEventHandler ErrorDataReceived;
    }
}
