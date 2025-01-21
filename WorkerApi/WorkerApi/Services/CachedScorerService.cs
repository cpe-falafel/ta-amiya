using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WorkerApi.Utils;

namespace WorkerApi.Services
{
    public class CachedScorerService: ICachedScorerService
    {
        private static string SCORE_KEY = "amiya_last_score";
        private static string MIN_SCORE_KEY = "amiya_min_score";

        private readonly IMemoryCache _cache;

        private string? _apiUrl;
        private string? _outJpg;
        private ILogger<ICachedScorerService> _logger;

        public CachedScorerService(IMemoryCache cache, ILogger<ICachedScorerService> logger)
        {
            _cache = cache;
            _apiUrl = Environment.GetEnvironmentVariable("AMIYA_API");
            _outJpg = Environment.GetEnvironmentVariable("AMIYA_OUTJPG");
            _logger = logger;
        }
        
        private async Task<string> SendFile(string filePath, string apiUrl)
        {
            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var fileContent = new StreamContent(fileStream))
                        {
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                            form.Add(fileContent, "image", Path.GetFileName(filePath));

                            var response = await httpClient.PostAsync(apiUrl, form);
                            response.EnsureSuccessStatusCode();

                            string responseBody = await response.Content.ReadAsStringAsync();
                            return responseBody;
                        }
                    }
                }
            }
        }

        public async Task ComputeScore()
        {
            if (_outJpg == null || _apiUrl == null) {
                _logger.LogWarning("Need AMIYA_API and AMIYA_OUTJPG to compute score");
                return;
            }
            if (!File.Exists(_outJpg))
            {
                _logger.LogWarning("File does not exist");
                return;
            }
            string? body;
            try
            {
                body = await SendFile(_outJpg, _apiUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while asking score : " + ex.Message);
                return;
            }
            if (body == null) { return; }
            var jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
            var scoreD = ConversionUtils.EnsureDouble(jsonDictionary.GetValueOrDefault("gore_score"));
            _logger.LogInformation("Got score :" + scoreD.ToString());
            if (scoreD >= 0 && scoreD<= uint.MaxValue)
            {
                _cache.Set(SCORE_KEY, ConversionUtils.ToStringInvariant((uint)scoreD));
            }
            else
            {
                _logger.LogError("Could not serialize score");
            }
        }

        public uint GetCachedScore()
        {
            if (!_cache.TryGetValue(SCORE_KEY, out string? cachedValue) || cachedValue == null)
            {
                return 0;
            }
            try
            {
                var score = ConversionUtils.EnsureInt(cachedValue);
                return score >= 0 ? (uint)score : 0;
            }
            catch
            {
                return 0;
            }
        }

        public uint GetCachedMinScore()
        {
            if (!_cache.TryGetValue(MIN_SCORE_KEY, out string? cachedValue) || cachedValue == null)
            {
                return 10; // By default: 10
            }
            try
            {
                var minScore = ConversionUtils.EnsureInt(cachedValue);
                return minScore >= 0 ? (uint)minScore : 10;
            }
            catch
            {
                return 10;
            }
        }

        public void SetMinScore(uint minScore)
        {
            _cache.Set(MIN_SCORE_KEY, ConversionUtils.ToStringInvariant(minScore));
        }
    }
}
