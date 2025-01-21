using Microsoft.Extensions.Caching.Memory;

namespace WorkerApi.Services
{
    public interface ICachedScorerService
    {
        public uint GetCachedScore();

        public Task ComputeScore();

        public void SetMinScore(uint minScore);
        public uint GetCachedMinScore();

    }
}
