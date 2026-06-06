namespace Fm2ndParser
{
    internal class CBlock : Block
    {
        public short Sound { get; set; }
        public bool Fails { get { return !Hits && !Uncond; } }
        public bool Hits { get; set; }
        public bool Uncond { get; set; }
        public bool Level { get { return !Skill; } }
        public bool Skill { get; set; }

        public byte From { get; internal set; }
        public byte To { get; internal set; }
        public object SkillNumber { get; internal set; }
    }
}