using Newtonsoft.Json;

namespace Fm2ndParser
{
    public abstract class Block
    {
        [JsonIgnore]
        public byte[] Data { get; set; }
        public int Index { get;  set; }
        public string Type { get; set; }
    }
}