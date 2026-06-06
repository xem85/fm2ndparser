namespace Fm2ndParser
{
    public class EBBlock : Block
    {
        public EBFadingType FadingType { get; set; }
        public Rgba Rgba { get; set; }
        public ushort Duration { get; set; }
        public bool Player { get; set; }
        public bool Enemy { get; set; }
        public bool BG { get; set; }
        public bool System { get; set; }
        public EBShakeBg ShakeBgX { get; set; }
        public EBShakeBg ShakeBgY { get; set; }
    }
}