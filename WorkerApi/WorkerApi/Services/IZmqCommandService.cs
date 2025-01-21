namespace WorkerApi.Services
{
    public interface IZmqCommandService
    {
        Task SendCommandAsync(bool applyBlur);
    }
}
