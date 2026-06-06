using System.Collections.Generic;

namespace Fm2ndParser
{
    internal class ComBlock : Block
    {
        public ushort Skill { get; set; }
        public byte SkillBlock { get; set; }
        public byte Time { get; set; }

        public ICollection<CommandStep> Steps { get; set; }
    }
}