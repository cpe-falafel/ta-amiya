using System.ComponentModel.DataAnnotations;

namespace WorkerApi.Models
{
    public class FilterParameter<TValue>
    {
        public string? Key { get; set; }
        public TValue? Value { get; set; }
    }
}
