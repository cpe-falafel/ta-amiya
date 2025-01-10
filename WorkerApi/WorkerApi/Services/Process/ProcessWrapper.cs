using System.Diagnostics;

namespace WorkerApi.Services.Process
{
    public class ProcessWrapper : IProcessWrapper
    {
        private readonly System.Diagnostics.Process _process;

        public ProcessWrapper() 
        {
            _process = new System.Diagnostics.Process();
        }
        public bool HasExited => _process.HasExited;
        public ProcessStartInfo StartInfo
        {
            get => _process.StartInfo;
            set => _process.StartInfo = value;
        }
        public bool EnableRaisingEvents
        {
            get => _process.EnableRaisingEvents;
            set => _process.EnableRaisingEvents = value;
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add => _process.OutputDataReceived += value;
            remove => _process.OutputDataReceived -= value;
        }

        public event DataReceivedEventHandler ErrorDataReceived
        {
            add => _process.ErrorDataReceived += value;
            remove => _process.ErrorDataReceived -= value;
        }

        public int ProcessId => _process.Id;

        public void Start() => _process.Start();
        public void Kill() => _process.Kill();
        public void BeginOutputReadLine() => _process.BeginOutputReadLine();
        public void BeginErrorReadLine() => _process.BeginErrorReadLine();
        public Task WaitForExitAsync() => _process.WaitForExitAsync();
        public void Dispose() => _process.Dispose();
    }
}
