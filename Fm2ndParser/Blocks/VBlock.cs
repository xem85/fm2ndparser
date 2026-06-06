namespace Fm2ndParser
{
    internal class VBlock : Block
    {
        public ushort MultiCondSkill { get; internal set; }
        public byte MultiCondSkillBlock { get; internal set; }
        public byte Var { get; internal set; }
        public string VarName { get; internal set; }
        public bool Replace { get; internal set; }
        public bool Add { get; internal set; }
        public bool ItsTheSame { get; internal set; }
        public bool ItsAbove { get; internal set; }
        public bool ItsBelow { get; internal set; }
        public bool UseEven { get; internal set; }
        public byte UseEvenVar { get; internal set; }
        public string UseEvenVarName { get; internal set; }
        public short Value { get; internal set; }
        public short MultiCondValue { get; internal set; }
    }
}