namespace WorkerApi.Services
{
    public interface IApiKeyValidatorService
    {
        bool IsValid (string apiKey);
    }
}
