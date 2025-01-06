namespace WorkerApi.Models.Graph
{

    public abstract class FilterVertex
    {
        
        public string Key { get; }
        public FilterVertex(String key)
        {
            Key = key;
        }

        public abstract string FilterName { get; }

        public abstract object[] GetFilterParams();

        public abstract string[] OutStreams { get; }

        public abstract string[] InStreams { get; }
    }
}
