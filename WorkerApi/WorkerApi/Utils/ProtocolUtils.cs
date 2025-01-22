namespace WorkerApi.Utils
{
    public class ProtocolUtils
    {
        public static bool IsRtmpProtocol(string? url)
        {
            return url != null && url.StartsWith("rtmp://");
        }
    }
}
