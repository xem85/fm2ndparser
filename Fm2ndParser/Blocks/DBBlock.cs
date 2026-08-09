namespace Fm2ndParser
{
    public class DBBlock : Block
    {
        public bool Fail { get; set; }
        public SkillBlockReference Skill { get; set; }
        public DBCondition Condition { get; set; }
    }
}