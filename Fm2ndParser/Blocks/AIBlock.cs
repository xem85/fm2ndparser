namespace Fm2ndParser
{
    public class AIBlock : Block
    {
        public byte Num { get; set; }
        public byte Time { get; set; }
        public ColorOption Option { get; set; }
        public AIFadingType FadingType { get; set; }
        public Rgba Rgba { get; set; }
    }
}