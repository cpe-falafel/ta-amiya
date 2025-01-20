using Microsoft.Extensions.Caching.Memory;

namespace WorkerApi.Services
{
    public interface ICachedScorerService
    {
        public uint GetCachedScore();
    }
}
