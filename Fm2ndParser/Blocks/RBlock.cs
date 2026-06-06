namespace Fm2ndParser
{
    public class RBlock : Block
    {
        public SkillReference HitsStand { get; set; }
        public SkillReference HitsCrouched { get; set; }
        public SkillReference HitsAir { get; set; }
        public SkillReference GuardStand { get; set; }
        public SkillReference GuardCrouched { get; set; }
        public SkillReference GuardAir { get; set; }
    }
}