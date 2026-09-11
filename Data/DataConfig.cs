using System.Collections.Generic;

namespace MyAPI.Data
{
    public class DataConfig<T> where T : class
    {
        public List<T> Data { get; set; }
    }
}