namespace Fm2ndParser
{
    public class IBlock : Block
    {
        public ushort Wait { get; set; }
        public ushort I { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public bool TurnX { get; set; }
        public bool TurnY { get; set; }
        public bool IgnoreDirection { get; set; }
    }
}