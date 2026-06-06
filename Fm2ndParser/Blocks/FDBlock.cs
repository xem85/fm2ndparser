namespace Fm2ndParser
{
    public class FDBlock:Block
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public byte Number { get; set; }
        public bool Collide { get; set; }
        public bool Damaged { get; set; }
        public bool Throw { get; set; }
        public byte DamageRate { get; set; }
        public bool DamageRateEnabled { get { return Damaged; } }
    }
}