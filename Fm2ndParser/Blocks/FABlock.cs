namespace Fm2ndParser
{
    internal class FABlock: Block
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public byte Number { get; set; }
        public bool Cancel { get; internal set; }
        public bool NoDetection { get; internal set; }
        public bool Combo { get; internal set; }
        public bool NoSkyDetection { get; internal set; }
        public bool GuardFail { get; internal set; }
        public bool DuringGuard { get; internal set; }
        public bool DuringReceipt { get; internal set; }
        public bool Halfed { get; internal set; }
        public byte Power { get; internal set; }
    }
}