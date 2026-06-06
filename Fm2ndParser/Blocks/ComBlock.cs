using System.Collections.Generic;

namespace Fm2ndParser
{
    public class ComBlock : Block
    {
        public SkillBlockReference Skill { get; set; }
        public byte Time { get; set; }

        public ICollection<BlockCommandStep> Steps { get; set; }
    }
}