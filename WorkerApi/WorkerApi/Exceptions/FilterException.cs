namespace WorkerApi.Exceptions
{
    public class FilterException : Exception
    {

        public FilterException(string message) : base(message){ }

        public FilterException(string message, Exception inner) : base(message,  inner) { }
    }
}
