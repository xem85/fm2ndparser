namespace Fm2ndParser
{
    internal class ColorBlock:Block
    {
        public ColorOption Option { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
        public bool AEnabled { get { return Option == ColorOption.CustomAlpha; } }
    }
}