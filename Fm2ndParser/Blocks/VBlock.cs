namespace Fm2ndParser
{
    public class VBlock : Block
    {
        public SkillBlockReference MultiCondSkill { get; set; }
        public byte Var { get; set; }
        public string VarName { get; set; }
        public bool Replace { get; set; }
        public bool Add { get; set; }
        public bool ItsTheSame { get; set; }
        public bool ItsAbove { get; set; }
        public bool ItsBelow { get; set; }
        public bool UseEven { get; set; }
        public byte UseEvenVar { get; set; }
        public string UseEvenVarName { get; set; }
        public short Value { get; set; }
        public short MultiCondValue { get; set; }
    }
}