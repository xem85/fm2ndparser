using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Fm2ndParser
{
    public class Command
    {
        public string Name { get; set; }
        public short Time { get; set; }
        public SkillReference AirSkill { get; set; }
        public SkillReference StandSkill { get; set; }
        public SkillReference StandFarSkill { get; set; }
        public SkillReference CrouchedSkill { get; set; }
        public ICollection<Block> Blocks { get; set; }
    }
}