namespace Fm2ndParser
{
    public class RCBlock : Block
    {
        public bool Out { get { return !In; } }
        public bool In { get; set; }
        public bool TurnX { get; set; }
        public bool TurnY { get; set; }
        public bool Same { get; set; }
        public ushort CommonImage { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
    }
}