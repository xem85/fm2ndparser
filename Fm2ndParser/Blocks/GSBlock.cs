namespace Fm2ndParser
{
    public class GSBlock:Block
    {
        public SkillBlockReference Skill { get; set; }
        public bool IsMore { get; set; }
        public byte Level { get; set; }
        public short Add { get; set; }
    }
}