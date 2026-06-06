namespace Fm2ndParser
{
    public class RPBlock:Block
    {
        public bool Out { get { return !In; } }
        public bool In { get; set; }
        public bool TurnX { get; set; }
        public SkillReference HitJunction { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
    }
}