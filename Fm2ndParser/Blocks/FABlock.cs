namespace Fm2ndParser
{
    public class FABlock: Block
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public byte Number { get; set; }
        public bool Cancel { get; set; }
        public bool NoDetection { get; set; }
        public bool Combo { get; set; }
        public bool NoSkyDetection { get; set; }
        public bool GuardFail { get; set; }
        public bool DuringGuard { get; set; }
        public bool DuringReceipt { get; set; }
        public bool Halfed { get; set; }
        public byte Power { get; set; }
    }
}