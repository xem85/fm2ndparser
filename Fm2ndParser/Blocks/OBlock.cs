namespace Fm2ndParser
{
    public class OBlock : Block
    {
        public bool In { get { return !Out && !Point; } }
        public bool Out { get; set; }
        public bool Point { get; set; }
        public bool UnCond { get; set; }
        public bool Shadow { get; set; }
        public bool Parent { get; set; }
        public bool PicXY { get; set; }
        public SkillBlockReference Skill { get; set; }
        public SkillBlockReference OutSkill { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public byte Number { get; set; }
        public byte Depth { get; set; }
        public bool DepthEnabled { get { return !Point; } }
    }
}