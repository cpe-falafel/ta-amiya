using Microsoft.Extensions.Caching.Memory;

namespace WorkerApi.Services
{
    public class CachedScorerService: ICachedScorerService
    {
        private static string SCORE_KEY = "amiya_last_score";

        private readonly IMemoryCache _cache;

        public CachedScorerService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private string ComputeScore()
        {
            return "50";
        }

        public uint GetCachedScore()
        {
            if (!_cache.TryGetValue(SCORE_KEY, out string? cachedValue) || cachedValue == null)
            {
                return 0;
            }
            try
            {
                return uint.Parse(cachedValue);
            }
            catch
            {
                return 0;
            }
        }
    }
}
