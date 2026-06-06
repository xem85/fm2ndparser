namespace Fm2ndParser
{
    public class CBlock : Block
    {
        public short Sound { get; set; }
        public bool Fails { get { return !Hits && !Uncond; } }
        public bool Hits { get; set; }
        public bool Uncond { get; set; }
        public bool LevelCancelCondition { get { return !SkillCancelCondition; } }
        public bool SkillCancelCondition { get; set; }

        public byte From { get; set; }
        public byte To { get; set; }
        public SkillReference Skill { get; set; }
    }

    public enum CBlockWhen
    {

    }
}