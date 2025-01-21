namespace WorkerApi.Models
{
    public class VideoCommand
    {
        public List<string> Args { get; } = new List<string>();

        public uint MinScore { get; set; } = 10;
    }
}
