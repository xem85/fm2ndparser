namespace Fm2ndParser
{
    internal class RndBlock : Block
    {
        public ushort RandomNum { get; set; }
        public ushort WhenItsAbove { get; set; }
        public ushort Skill { get; set; }
        public byte SkillBlock { get; set; }
    }
}