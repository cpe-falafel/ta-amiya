namespace WorkerApi.Services
{
    public class ApiKeyValidatorService : IApiKeyValidatorService
    {
        private readonly string? _validApiKey;

        public ApiKeyValidatorService(IConfiguration configuration)
        {
            _validApiKey = Environment.GetEnvironmentVariable("AMIYA_APIKEY");
        }

        public bool IsValid(string apiKey)
        {
            if (apiKey == null) 
            {
                return true;
            }

            return apiKey == _validApiKey;
        }
    }
}