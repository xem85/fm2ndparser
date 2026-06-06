namespace Fm2ndParser
{
    public class ColorBlock:Block
    {
        public ColorOption Option { get; set; }
        public Rgba Rgba { get; set; }
        public bool AEnabled { get { return Option == ColorOption.CustomAlpha; } }
    }
}