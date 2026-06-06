namespace Fm2ndParser
{
    internal class OBlock : Block
    {
        public bool In { get { return !Out && !Point; } }
        public bool Out { get; internal set; }
        public bool Point { get; internal set; }
        public bool UnCond { get; internal set; }
        public bool Shadow { get; internal set; }
        public bool Parent { get; internal set; }
        public bool PicXY { get; internal set; }
        public ushort Skill { get; internal set; }
        public byte SkillBlock { get; internal set; }
        public ushort OutSkill { get; internal set; }
        public byte OutSkillBlock { get; internal set; }
        public short X { get; internal set; }
        public short Y { get; internal set; }
        public byte Number { get; internal set; }
        public byte Depth { get; internal set; }
        public bool DepthEnabled { get { return !Point; } }
    }
}