namespace Fm2ndParser
{
    public abstract class Block
    {
        public byte[] Data { get; set; }
        public string Type { get; set; }
    }
}