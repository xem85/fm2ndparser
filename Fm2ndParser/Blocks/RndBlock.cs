namespace Fm2ndParser
{
    public class RndBlock : Block
    {
        public ushort RandomNum { get; set; }
        public ushort WhenItsAbove { get; set; }
        public SkillBlockReference Skill { get; set; }
    }
}