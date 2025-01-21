namespace WorkerApi.Services
{
    public interface IZmqCommandService
    {
        Task<string> SendCommandAsync(bool applyBlur);
    }
}
