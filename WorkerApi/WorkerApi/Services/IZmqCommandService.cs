namespace WorkerApi.Services
{
    public interface IZmqCommandService
    {
        void SendCommand(bool applyBlur);
    }
}
