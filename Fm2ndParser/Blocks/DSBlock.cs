namespace Fm2ndParser
{
    internal class DSBlock : Block
    {
        public DSSkill When { get; set; }
        public ushort Skill { get; set; }
        public byte SkillBlock { get; set; }
    }
}